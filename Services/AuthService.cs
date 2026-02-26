using System;
using System.IO;
using System.Threading.Tasks;
using Supabase;
using MovieRate.Models;

namespace MovieRate.Services;

public class AuthService
{
    private readonly Client _client;

    public Supabase.Gotrue.User? CurrentUser => _client.Auth.CurrentUser;
    public bool IsLoggedIn => CurrentUser != null;

    public AuthService()
    {
        var config = ConfigService.GetSupabaseConfig();
        _client = new Client(config.Url, config.AnonKey, new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false,
            SessionHandler = new SupabaseSessionHandler()
        });
    }

    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
        _client.Auth.LoadSession();
        await _client.Auth.RetrieveSessionAsync();
    }
    
    public async Task<(bool success, string error)> RegisterAsync(string email, string password, string displayName)
    {
        try
        {
            var response = await _client.Auth.SignUp(email, password);
            return (response?.User != null, response?.User == null ? "Registration failed. Please try again." : string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ParseErrorMessage(ex.Message));
        }
    }

    public async Task<(bool success, string error)> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _client.Auth.SignIn(email, password);
            return (response?.User != null, response?.User == null ? "Login failed. Please try again." : string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ParseErrorMessage(ex.Message));
        }
    }

    private string ParseErrorMessage(string raw)
    {
        if (raw.Contains("Invalid login credentials")) return "Incorrect email or password.";
        if (raw.Contains("Email not confirmed")) return "Please confirm your email before logging in.";
        if (raw.Contains("User already registered")) return "An account with this email already exists.";
        if (raw.Contains("Password should be at least")) return "Password must be at least 6 characters.";
        if (raw.Contains("Unable to validate email address")) return "Please enter a valid email address.";
        if (raw.Contains("Network")) return "Network error. Please check your connection.";
        return "Something went wrong. Please try again.";
    }

    public async Task LogoutAsync()
    {
        await _client.Auth.SignOut();
    }

    public Client GetClient() => _client;
}

public class SupabaseSessionHandler : Supabase.Gotrue.Interfaces.IGotrueSessionPersistence<Supabase.Gotrue.Session>
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MovieRate", "session.json");

    public void SaveSession(Supabase.Gotrue.Session session)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, System.Text.Json.JsonSerializer.Serialize(session));
    }

    public Supabase.Gotrue.Session? LoadSession()
    {
        if (!File.Exists(_path)) return null;
        var json = File.ReadAllText(_path);
        return System.Text.Json.JsonSerializer.Deserialize<Supabase.Gotrue.Session>(json);
    }

    public void DestroySession()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }
}