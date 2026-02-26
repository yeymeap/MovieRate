using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MovieRate.Models;

public partial class Movie : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _tmdbId = string.Empty;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _posterUrl = string.Empty;
    [ObservableProperty] private int _rating = 0;
    [ObservableProperty] private string _category = string.Empty;
    [ObservableProperty] private WatchedStatus _watchedStatus = WatchedStatus.Unwatched;
    [ObservableProperty] private string _addedBy = string.Empty;
    [ObservableProperty] private DateTimeOffset _addedAt = DateTimeOffset.UtcNow;

    public Func<int, System.Threading.Tasks.Task>? RatingChangedCallback { get; set; }

    partial void OnRatingChanged(int value)
    {
        RatingChangedCallback?.Invoke(value);
    }
    
    public Func<WatchedStatus, System.Threading.Tasks.Task>? WatchedStatusChangedCallback { get; set; }

    partial void OnWatchedStatusChanged(WatchedStatus value)
    {
        WatchedStatusChangedCallback?.Invoke(value);
    }
}

public enum WatchedStatus
{
    Unwatched,
    Watching,
    Watched
}