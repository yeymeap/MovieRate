using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieRate.Services;
using System;
using System.Threading.Tasks;

namespace MovieRate.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _isRegisterMode = false;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    public event Action? OnLoginSuccess;

    public string SubmitButtonText => IsRegisterMode ? "Create Account" : "Sign In";
    public string ToggleButtonText => IsRegisterMode ? "Already have an account? Sign in" : "No account? Register";
    
    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        ErrorMessage = string.Empty;

        if (IsRegisterMode && Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsLoading = true;

        var (success, error) = IsRegisterMode
            ? await _authService.RegisterAsync(Email, Password, DisplayName)
            : await _authService.LoginAsync(Email, Password);

        IsLoading = false;

        if (success)
            OnLoginSuccess?.Invoke();
        else
            ErrorMessage = error;
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(SubmitButtonText));
        OnPropertyChanged(nameof(ToggleButtonText));
    }
}