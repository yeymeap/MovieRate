using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MovieRate.Services;

namespace MovieRate.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly AuthService _authService;

    public string WelcomeMessage => $"Welcome, {_authService.CurrentUser?.Email ?? "user"}!";
    public event Action? OnLogout;

    public HomeViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _authService.LogoutAsync();
        OnLogout?.Invoke();
    }
}