using CommunityToolkit.Mvvm.ComponentModel;
using MovieRate.Services;
using MovieRate.ViewModels;

namespace MovieRate.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AuthService _authService;

    [ObservableProperty] private ViewModelBase _currentView;

    public MainWindowViewModel()
    {
        _authService = new AuthService();

        if (_authService.IsLoggedIn)
            CurrentView = new HomeViewModel(_authService);
        else
        {
            var loginVm = new LoginViewModel(_authService);
            loginVm.OnLoginSuccess += HandleLoginSuccess;
            CurrentView = loginVm;
        }
    }

    private void HandleLoginSuccess()
    {
        var homeVm = new HomeViewModel(_authService);
        homeVm.OnLogout += HandleLogout;
        CurrentView = homeVm;
    }

    private void HandleLogout()
    {
        var loginVm = new LoginViewModel(_authService);
        loginVm.OnLoginSuccess += HandleLoginSuccess;
        CurrentView = loginVm;
    }
}