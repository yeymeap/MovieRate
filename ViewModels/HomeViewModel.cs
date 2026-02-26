using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieRate.Models;
using MovieRate.Services;

namespace MovieRate.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;

    [ObservableProperty] private ObservableCollection<MovieList> _lists = new();
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _newListName = string.Empty;
    [ObservableProperty] private string _shareEmail = string.Empty;
    [ObservableProperty] private string _shareMessage = string.Empty;
    [ObservableProperty] private MovieList? _listToShare = null;

    public string WelcomeMessage => $"Welcome, {_authService.CurrentUser?.Email ?? "user"}!";

    public event Action? OnLogout;

    public HomeViewModel(AuthService authService)
    {
        _authService = authService;
        _supabaseService = new SupabaseService(authService);
        _ = LoadListsAsync();
    }

    private async Task LoadListsAsync()
    {
        IsLoading = true;
        var lists = await _supabaseService.GetListsAsync();
        Lists = new ObservableCollection<MovieList>(lists);
        IsLoading = false;
    }

    [RelayCommand]
    private async Task CreateListAsync()
    {
        if (string.IsNullOrWhiteSpace(NewListName)) return;
        var list = await _supabaseService.CreateListAsync(NewListName);
        if (list != null)
        {
            Lists.Add(list);
            NewListName = string.Empty;
        }
    }

    [RelayCommand]
    private async Task DeleteListAsync(MovieList list)
    {
        await _supabaseService.DeleteListAsync(list.Id);
        Lists.Remove(list);
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _authService.LogoutAsync();
        OnLogout?.Invoke();
    }
    
    [ObservableProperty] private ListViewModel? _selectedList;

    [RelayCommand]
    private void SelectList(MovieList list)
    {
        Console.WriteLine($"Selected list: {list.Name}");
        SelectedList = new ListViewModel(_authService, _supabaseService, list);
    }
    
    [RelayCommand]
    private void StartShare(MovieList list)
    {
        ListToShare = list;
        ShareEmail = string.Empty;
        ShareMessage = string.Empty;
    }

    [RelayCommand]
    private void CancelShare()
    {
        ListToShare = null;
        ShareEmail = string.Empty;
        ShareMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ShareListAsync()
    {
        if (ListToShare == null || string.IsNullOrWhiteSpace(ShareEmail)) return;

        var profile = await _supabaseService.GetProfileByEmailAsync(ShareEmail);
        if (profile == null)
        {
            ShareMessage = "No user found with that email.";
            return;
        }

        if (profile.Id == _authService.CurrentUser?.Id)
        {
            ShareMessage = "You can't share a list with yourself.";
            return;
        }

        await _supabaseService.ShareListAsync(ListToShare.Id, profile.Id);
        ShareMessage = $"List shared with {ShareEmail} successfully!";
        await Task.Delay(2000);
        ListToShare = null;
        ShareMessage = string.Empty;
    }
}