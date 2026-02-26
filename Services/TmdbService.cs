using System;
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
            movies.Add(new TmdbMovie
            {
                TmdbId = item.GetProperty("id").GetInt32().ToString(),
                Title = item.GetProperty("title").GetString() ?? string.Empty,
                PosterUrl = posterPath != null ? $"{ImageBaseUrl}{posterPath}" : string.Empty,
                Overview = item.TryGetProperty("overview", out var overview) ? overview.GetString() ?? string.Empty : string.Empty,
                ReleaseDate = item.TryGetProperty("release_date", out var date) ? date.GetString() ?? string.Empty : string.Empty
            });
        }
        return movies;
    }
}