using System;

namespace MovieRate.Models;

public class Movie
{
    public string Id { get; set; } = string.Empty;
    public string TmdbId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public int Rating { get; set; } = 0;
    public string Category { get; set; } = string.Empty;
    public WatchedStatus WatchedStatus { get; set; } = WatchedStatus.Unwatched;
    public string AddedBy { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public enum WatchedStatus
{
    Unwatched,
    Watching,
    Watched
}