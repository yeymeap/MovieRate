using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
    private readonly TmdbService _tmdbService = new();
    private readonly MovieList _list;
    private CancellationTokenSource? _searchCts;
    private List<Movie> _allMovies = new();

    [ObservableProperty] private ObservableCollection<Movie> _movies = new();
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _newMovieTitle = string.Empty;
    [ObservableProperty] private string _newMovieCategory = string.Empty;
    [ObservableProperty] private ObservableCollection<TmdbMovie> _searchResults = new();
    [ObservableProperty] private bool _isSearching = false;
    [ObservableProperty] private bool _showSearchResults = false;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _sortBy = "Date Added";
    [ObservableProperty] private string _searchQuery = string.Empty;
    
    public bool HasNoMovies => Movies.Count == 0;
    public string ListName => _list.Name;

    public ListViewModel(AuthService authService, SupabaseService supabaseService, MovieList list)
    {
        _authService = authService;
        _supabaseService = supabaseService;
        _list = list;
        _ = LoadMoviesAsync();
    }

    private Movie AttachRatingCallback(Movie movie)
    {
        movie.RatingChangedCallback = async (rating) =>
        {
            await _supabaseService.UpdateMovieRatingAsync(movie.Id, rating);
        };
        return movie;
    }
    
    private Movie AttachCallbacks(Movie movie)
    {
        movie.RatingChangedCallback = async (rating) =>
        {
            await _supabaseService.UpdateMovieRatingAsync(movie.Id, rating);
        };
        movie.WatchedStatusChangedCallback = async (status) =>
        {
            await _supabaseService.UpdateMovieWatchedStatusAsync(movie.Id, status);
        };
        return movie;
    }

    private async Task LoadMoviesAsync()
    {
        IsLoading = true;
        _allMovies = await _supabaseService.GetMoviesAsync(_list.Id);
        _allMovies = _allMovies.Select(AttachCallbacks).ToList();
        ApplyFilterAndSort();
        Movies.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoMovies));
        OnPropertyChanged(nameof(HasNoMovies));
        IsLoading = false;
    }

    [RelayCommand]
    private async Task AddMovieAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMovieTitle)) return;
        var movie = await _supabaseService.AddMovieAsync(_list.Id, NewMovieTitle, NewMovieCategory);
        if (movie != null)
        {
            Movies.Add(AttachCallbacks(movie));
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

    [RelayCommand]
    private async Task UpdateRatingAsync((Movie movie, int rating) args)
    {
        args.movie.Rating = args.rating;
        await _supabaseService.UpdateMovieRatingAsync(args.movie.Id, args.rating);
    }

    [RelayCommand]
    private async Task SearchMoviesAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMovieTitle)) return;
        IsSearching = true;
        ShowSearchResults = true;
        var results = await _tmdbService.SearchMoviesAsync(NewMovieTitle);
        SearchResults = new ObservableCollection<TmdbMovie>(results);
        IsSearching = false;
    }

    [RelayCommand]
    private async Task SelectTmdbMovieAsync(TmdbMovie tmdbMovie)
    {
        foreach (var existing in Movies)
        {
            if (existing.TmdbId == tmdbMovie.TmdbId)
            {
                ShowSearchResults = false;
                NewMovieTitle = string.Empty;
                SearchResults.Clear();
                StatusMessage = $"{tmdbMovie.Title} is already in this list.";
                await Task.Delay(3000);
                StatusMessage = string.Empty;
                return;
            }
        }

        ShowSearchResults = false;
        var movie = await _supabaseService.AddMovieAsync(
            _list.Id,
            tmdbMovie.Title,
            tmdbMovie.Genres,
            tmdbMovie.TmdbId,
            tmdbMovie.PosterUrl);
        if (movie != null)
        {
            Movies.Add(AttachCallbacks(movie));
            NewMovieTitle = string.Empty;
            NewMovieCategory = string.Empty;
            SearchResults.Clear();
        }
    }

    partial void OnNewMovieTitleChanged(string value)
    {
        _ = AutoSearchAsync(value);
    }

    private async Task AutoSearchAsync(string query)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        if (string.IsNullOrWhiteSpace(query))
        {
            ShowSearchResults = false;
            SearchResults.Clear();
            return;
        }

        try
        {
            await Task.Delay(400, token);
            if (token.IsCancellationRequested) return;
            IsSearching = true;
            ShowSearchResults = true;
            var results = await _tmdbService.SearchMoviesAsync(query);
            if (token.IsCancellationRequested) return;
            SearchResults = new ObservableCollection<TmdbMovie>(results);
            IsSearching = false;
        }
        catch (TaskCanceledException)
        {
        }
    }
    
    [RelayCommand]
    private void ToggleWatchedStatus(Movie movie)
    {
        movie.WatchedStatus = movie.WatchedStatus switch
        {
            WatchedStatus.Unwatched => WatchedStatus.Watching,
            WatchedStatus.Watching => WatchedStatus.Watched,
            WatchedStatus.Watched => WatchedStatus.Unwatched,
            _ => WatchedStatus.Unwatched
        };
    }
    
    public IEnumerable<string> SortOptions => new[]
    {
        "Date Added", "Title", "Rating", "Watched Status"
    };

    partial void OnSortByChanged(string value)
    {
        ApplyFilterAndSort();
    }

    private void ApplyFilterAndSort()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? _allMovies
            : _allMovies.Where(m => m.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

        var sorted = SortBy switch
        {
            "Title" => filtered.OrderBy(m => m.Title),
            "Rating" => filtered.OrderByDescending(m => m.Rating),
            "Watched Status" => filtered.OrderBy(m => m.WatchedStatus),
            _ => filtered.OrderBy(m => m.AddedAt)
        };

        Movies = new ObservableCollection<Movie>(sorted);
        Movies.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoMovies));
        OnPropertyChanged(nameof(HasNoMovies));
    }
    
    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilterAndSort();
    }
}