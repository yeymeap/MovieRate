using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MovieRate.Models;

namespace MovieRate.Services;

public class TmdbService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.themoviedb.org/3";
    private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";
    private static readonly Dictionary<int, string> GenreMap = new()
    {
        { 28, "Action" }, { 12, "Adventure" }, { 16, "Animation" },
        { 35, "Comedy" }, { 80, "Crime" }, { 99, "Documentary" },
        { 18, "Drama" }, { 10751, "Family" }, { 14, "Fantasy" },
        { 36, "History" }, { 27, "Horror" }, { 10402, "Music" },
        { 9648, "Mystery" }, { 10749, "Romance" }, { 878, "Sci-Fi" },
        { 10770, "TV Movie" }, { 53, "Thriller" }, { 10752, "War" },
        { 37, "Western" }
    };
    
    public TmdbService()
    {
        _httpClient = new HttpClient();
        _apiKey = ConfigService.GetTmdbConfig().ApiKey;
    }

    public async Task<List<TmdbMovie>> SearchMoviesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<TmdbMovie>();

        var url = $"{BaseUrl}/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}";
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);
        var results = json.RootElement.GetProperty("results");

        var movies = new List<TmdbMovie>();
        foreach (var item in results.EnumerateArray())
        {
            var posterPath = item.TryGetProperty("poster_path", out var poster) ? poster.GetString() : null;

            var genreIds = item.TryGetProperty("genre_ids", out var genres)
                ? genres.EnumerateArray().Select(g => g.GetInt32()).ToList()
                : new List<int>();

            var genreNames = string.Join(", ", genreIds
                .Where(id => GenreMap.ContainsKey(id))
                .Select(id => GenreMap[id]));

            movies.Add(new TmdbMovie
            {
                TmdbId = item.GetProperty("id").GetInt32().ToString(),
                Title = item.GetProperty("title").GetString() ?? string.Empty,
                PosterUrl = posterPath != null ? $"{ImageBaseUrl}{posterPath}" : string.Empty,
                Overview = item.TryGetProperty("overview", out var overview) ? overview.GetString() ?? string.Empty : string.Empty,
                ReleaseDate = item.TryGetProperty("release_date", out var date) ? date.GetString() ?? string.Empty : string.Empty,
                Genres = genreNames
            });
        }
        return movies;
    }
    
    public async Task<TmdbMovie?> GetMovieDetailsAsync(string tmdbId)
    {
        if (string.IsNullOrWhiteSpace(tmdbId)) return null;

        var url = $"{BaseUrl}/movie/{tmdbId}?api_key={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        var posterPath = json.RootElement.TryGetProperty("poster_path", out var poster) ? poster.GetString() : null;
        var genres = json.RootElement.TryGetProperty("genres", out var genreArray)
            ? string.Join(", ", genreArray.EnumerateArray().Select(g => g.GetProperty("name").GetString()))
            : string.Empty;

        return new TmdbMovie
        {
            TmdbId = tmdbId,
            Title = json.RootElement.GetProperty("title").GetString() ?? string.Empty,
            PosterUrl = posterPath != null ? $"{ImageBaseUrl}{posterPath}" : string.Empty,
            Overview = json.RootElement.TryGetProperty("overview", out var overview) ? overview.GetString() ?? string.Empty : string.Empty,
            ReleaseDate = json.RootElement.TryGetProperty("release_date", out var date) ? date.GetString() ?? string.Empty : string.Empty,
            Genres = genres,
            Runtime = json.RootElement.TryGetProperty("runtime", out var runtime) ? runtime.GetInt32() : 0

        };
    }
}