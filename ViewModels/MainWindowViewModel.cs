using CommunityToolkit.Mvvm.ComponentModel;
using MovieRate.Services;

namespace MovieRate.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AuthService _authService;

    [ObservableProperty] private ViewModelBase _currentView;

    public MainWindowViewModel(AuthService authService)
    {
        _authService = authService;

        if (_authService.IsLoggedIn)
        {
            var homeVm = new HomeViewModel(_authService);
            homeVm.OnLogout += HandleLogout;
            _currentView = homeVm;
        }
        else
        {
            var loginVm = new LoginViewModel(_authService);
            loginVm.OnLoginSuccess += HandleLoginSuccess;
            _currentView = loginVm;
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