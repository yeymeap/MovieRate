using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieRate.Models;
using MovieRate.Services;

namespace MovieRate.ViewModels;

public partial class MovieDetailViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;
    private readonly TmdbService _tmdbService = new();
    private readonly Movie _movie;
    private readonly bool _isListOwner;

    [ObservableProperty] private string _overview = string.Empty;
    [ObservableProperty] private string _genres = string.Empty;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private ObservableCollection<MovieComment> _comments = new();
    [ObservableProperty] private string _newComment = string.Empty;
    [ObservableProperty] private ObservableCollection<MemberMovieData> _memberData = new();
    [ObservableProperty] private int _rating;
    [ObservableProperty] private WatchedStatus _watchedStatus;

    public string Title => _movie.Title;
    public string PosterUrl => _movie.PosterUrl;
    public string ReleaseDate => _movie.ReleaseDate;
    public string AddedByEmail => _movie.AddedByEmail;
    public Movie Movie => _movie;
    
    public event Action? OnBack;

    private readonly List<string> _memberIds;

    public MovieDetailViewModel(AuthService authService, SupabaseService supabaseService, Movie movie, bool isListOwner, List<string> memberIds)
    {
        _authService = authService;
        _supabaseService = supabaseService;
        _movie = movie;
        _isListOwner = isListOwner;
        _memberIds = memberIds;
        _rating = movie.Rating;
        _watchedStatus = movie.WatchedStatus;
        _ = LoadDetailsAsync();
    }

    partial void OnRatingChanged(int value)
    {
        // Update the movie object so list view stays in sync
        _movie.Rating = value;
        _ = SaveAndReloadAsync();
    }

    partial void OnWatchedStatusChanged(WatchedStatus value)
    {
        _movie.WatchedStatus = value;
        _ = SaveWatchedStatusAsync(value);
    }

    private async Task SaveAndReloadAsync()
    {
        await _supabaseService.UpdateMovieRatingAsync(_movie.Id, _movie.TmdbId, Rating);
        await ReloadMemberDataAsync();
    }

    private async Task SaveWatchedStatusAsync(WatchedStatus status)
    {
        await _supabaseService.UpdateMovieWatchedStatusAsync(_movie.Id, _movie.TmdbId, status);
        await ReloadMemberDataAsync();
    }

    [RelayCommand]
    private void ToggleWatchedStatus()
    {
        WatchedStatus = WatchedStatus == WatchedStatus.Unwatched
            ? WatchedStatus.Watched
            : WatchedStatus.Unwatched;
    }

    private async Task LoadDetailsAsync()
    {
        IsLoading = true;

        if (!string.IsNullOrWhiteSpace(_movie.TmdbId))
        {
            var details = await _tmdbService.GetMovieDetailsAsync(_movie.TmdbId);
            if (details != null)
            {
                Overview = details.Overview;
                Genres = details.Genres;
            }
        }

        await LoadCommentsAsync();
        await ReloadMemberDataAsync();

        IsLoading = false;
    }

    public async Task ReloadMemberDataAsync()
    {
        var currentUserId = _authService.CurrentUser?.Id ?? string.Empty;
        var allRatings = await _supabaseService.GetAllMemberTmdbRatingsAsync(_movie.TmdbId, _memberIds);
        var memberDataList = new ObservableCollection<MemberMovieData>();
        foreach (var data in allRatings)
        {
            if (data.UserId == currentUserId) continue;
            var displayName = await _supabaseService.GetDisplayNameAsync(data.UserId);
            memberDataList.Add(new MemberMovieData
            {
                DisplayName = displayName,
                Rating = data.Rating,
                WatchedStatus = Enum.Parse<WatchedStatus>(data.WatchedStatus)
            });
        }
        MemberData = memberDataList;
    }

    private async Task LoadCommentsAsync()
    {
        var currentUserId = _authService.CurrentUser?.Id ?? string.Empty;
        var rawComments = await _supabaseService.GetCommentsAsync(_movie.Id);
        var comments = new ObservableCollection<MovieComment>();

        foreach (var c in rawComments)
        {
            var displayName = await _supabaseService.GetDisplayNameAsync(c.UserId);
            comments.Add(new MovieComment
            {
                Id = c.Id,
                MovieId = c.MovieId,
                UserId = c.UserId,
                DisplayName = displayName,
                Content = c.Content,
                IsOwn = c.UserId == currentUserId,
                CanDelete = c.UserId == currentUserId || _isListOwner
            });
        }
        Comments = comments;
    }

    [RelayCommand]
    private async Task AddCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(NewComment)) return;
        var comment = await _supabaseService.AddCommentAsync(_movie.Id, NewComment);
        if (comment != null)
        {
            var currentUserId = _authService.CurrentUser?.Id ?? string.Empty;
            var displayName = await _supabaseService.GetDisplayNameAsync(comment.UserId);
            Comments.Add(new MovieComment
            {
                Id = comment.Id,
                MovieId = comment.MovieId,
                UserId = comment.UserId,
                DisplayName = displayName,
                Content = comment.Content,
                IsOwn = true,
                CanDelete = true
            });
            NewComment = string.Empty;
        }
    }

    [RelayCommand]
    private async Task DeleteCommentAsync(MovieComment comment)
    {
        await _supabaseService.DeleteCommentAsync(comment.Id);
        Comments.Remove(comment);
    }

    [RelayCommand]
    private void GoBack()
    {
        OnBack?.Invoke();
    }
}

public class MemberMovieData
{
    public string DisplayName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public WatchedStatus WatchedStatus { get; set; }
}