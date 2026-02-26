using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;

namespace MovieRate.Services;

public class AuthService
{
    private readonly FirebaseAuthClient _client;
    private UserCredential? _currentCredential;
    private User? _persistedUser;

    public User? CurrentUser => _currentCredential?.User ?? _persistedUser;
    public bool IsLoggedIn => CurrentUser != null;

    public AuthService()
    {
        var config = ConfigService.GetFirebaseConfig();

        var authConfig = new FirebaseAuthConfig
        {
            ApiKey = config.ApiKey,
            AuthDomain = config.AuthDomain,
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider()
            },
            UserRepository = new FileUserRepository("auth")
        };

        _client = new FirebaseAuthClient(authConfig);
        _persistedUser = _client.User;
    }

    public async Task<(bool success, string error)> RegisterAsync(string email, string password, string displayName)
    {
        try
        {
            _currentCredential = await _client.CreateUserWithEmailAndPasswordAsync(email, password, displayName);
            return (true, string.Empty);
        }
        catch (FirebaseAuthException ex)
        {
            return (false, ParseFirebaseError(ex.Reason));
        }
    }

    public async Task<(bool success, string error)> LoginAsync(string email, string password)
    {
        try
        {
            _currentCredential = await _client.SignInWithEmailAndPasswordAsync(email, password);
            return (true, string.Empty);
        }
        catch (FirebaseAuthException ex)
        {
            return (false, ParseFirebaseError(ex.Reason));
        }
    }

    public void Logout()
    {
        _client.SignOut();
        _currentCredential = null;
        _persistedUser = null;
    }

    private string ParseFirebaseError(AuthErrorReason reason) => reason switch
    {
        AuthErrorReason.WrongPassword => "Incorrect password.",
        AuthErrorReason.UnknownEmailAddress => "No account found with that email.",
        AuthErrorReason.EmailExists => "An account with this email already exists.",
        AuthErrorReason.WeakPassword => "Password is too weak.",
        AuthErrorReason.InvalidEmailAddress => "Invalid email address.",
        _ => "Something went wrong. Please try again."
    };
}