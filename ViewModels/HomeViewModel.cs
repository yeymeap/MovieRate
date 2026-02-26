using System;
using CommunityToolkit.Mvvm.Input;
using MovieRate.Services;

namespace MovieRate.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly AuthService _authService;

    public string WelcomeMessage => $"Welcome, {_authService.CurrentUser?.Info?.DisplayName ?? "user"}!";

    public event Action? OnLogout;

    public HomeViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        OnLogout?.Invoke();
    }
}