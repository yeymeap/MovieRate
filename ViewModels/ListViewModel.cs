using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieRate.Models;
using MovieRate.Services;

namespace MovieRate.ViewModels;

public partial class ListViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;
    private readonly MovieList _list;

    [ObservableProperty] private ObservableCollection<Movie> _movies = new();
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _newMovieTitle = string.Empty;
    [ObservableProperty] private string _newMovieCategory = string.Empty;

    public string ListName => _list.Name;

    public ListViewModel(AuthService authService, SupabaseService supabaseService, MovieList list)
    {
        _authService = authService;
        _supabaseService = supabaseService;
        _list = list;
        _ = LoadMoviesAsync();
    }

    private async Task LoadMoviesAsync()
    {
        IsLoading = true;
        var movies = await _supabaseService.GetMoviesAsync(_list.Id);
        Movies = new ObservableCollection<Movie>(movies as IEnumerable<Movie>);
        IsLoading = false;
    }

    [RelayCommand]
    private async Task AddMovieAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMovieTitle)) return;
        var movie = await _supabaseService.AddMovieAsync(_list.Id, NewMovieTitle, NewMovieCategory);
        if (movie != null)
        {
            Movies.Add(movie);
            NewMovieTitle = string.Empty;
            NewMovieCategory = string.Empty;
        }
    }

    [RelayCommand]
    private async Task DeleteMovieAsync(Movie movie)
    {
        await _supabaseService.DeleteMovieAsync(movie.Id);
        Movies.Remove(movie);
    }
}