namespace MovieRate.Models;

public class TmdbMovie
{
    public string TmdbId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string ReleaseDate { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty;
}