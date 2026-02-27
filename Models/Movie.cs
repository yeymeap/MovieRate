using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MovieRate.Models;

public partial class Movie : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _tmdbId = string.Empty;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _posterUrl = string.Empty;
    [ObservableProperty] private string _category = string.Empty;
    [ObservableProperty] private string _releaseDate = string.Empty;
    [ObservableProperty] private string _addedBy = string.Empty;
    [ObservableProperty] private string _addedByEmail = string.Empty;
    [ObservableProperty] private DateTimeOffset _addedAt = DateTimeOffset.UtcNow;

    // Current user's personal data
    [ObservableProperty] private int _rating = 0;
    [ObservableProperty] private WatchedStatus _watchedStatus = WatchedStatus.Unwatched;

    // All members' data for detail view
    public Dictionary<string, (int Rating, WatchedStatus Status, string DisplayName)> MemberData { get; set; } = new();

    public Func<int, System.Threading.Tasks.Task>? RatingChangedCallback { get; set; }
    public Func<WatchedStatus, System.Threading.Tasks.Task>? WatchedStatusChangedCallback { get; set; }

    partial void OnRatingChanged(int value)
    {
        RatingChangedCallback?.Invoke(value);
    }

    partial void OnWatchedStatusChanged(WatchedStatus value)
    {
        WatchedStatusChangedCallback?.Invoke(value);
    }
}

public enum WatchedStatus
{
    Unwatched,
    Watched
}