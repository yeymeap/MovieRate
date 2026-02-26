using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieRate.Models;
using Supabase;

namespace MovieRate.Services;

public class SupabaseService
{
    private readonly Client _client;
    private readonly AuthService _authService;

    public SupabaseService(AuthService authService)
    {
        _authService = authService;
        _client = authService.GetClient();
    }

    public async Task<List<MovieList>> GetListsAsync()
    {
        var response = await _client
            .From<SupabaseList>()
            .Get();

        var lists = new List<MovieList>();
        foreach (var item in response.Models)
        {
            lists.Add(new MovieList
            {
                Id = item.Id,
                Name = item.Name,
                OwnerId = item.OwnerId,
                CreatedAt = item.CreatedAt
            });
        }
        return lists;
    }

    public async Task<MovieList?> CreateListAsync(string name)
    {
        var userId = _authService.CurrentUser?.Id ?? string.Empty;
        var newList = new SupabaseList
        {
            Name = name,
            OwnerId = userId
        };

        var response = await _client
            .From<SupabaseList>()
            .Insert(newList);

        var item = response.Models[0];
        return new MovieList
        {
            Id = item.Id,
            Name = item.Name,
            OwnerId = item.OwnerId,
            CreatedAt = item.CreatedAt
        };
    }

    public async Task DeleteListAsync(string listId)
    {
        await _client
            .From<SupabaseList>()
            .Where(x => x.Id == listId)
            .Delete();
    }
    
    public async Task<List<Movie>> GetMoviesAsync(string listId)
    {
        var response = await _client
            .From<SupabaseMovie>()
            .Where(x => x.ListId == listId)
            .Get();

        var movies = new List<Movie>();
        foreach (var item in response.Models)
        {
            movies.Add(new Movie
            {
                Id = item.Id,
                TmdbId = item.TmdbId,
                Title = item.Title,
                PosterUrl = item.PosterUrl,
                Rating = item.Rating,
                Category = item.Category,
                WatchedStatus = Enum.Parse<WatchedStatus>(item.WatchedStatus),
                AddedBy = item.AddedBy,
                AddedAt = item.AddedAt
            });
        }
        return movies;
    }
    
    public async Task<Movie?> AddMovieAsync(string listId, string title, string category)
    {
        var userId = _authService.CurrentUser?.Id ?? string.Empty;
        var newMovie = new SupabaseMovie
        {
            ListId = listId,
            Title = title,
            Category = category,
            WatchedStatus = "Unwatched",
            AddedBy = userId
        };

        var response = await _client
            .From<SupabaseMovie>()
            .Insert(newMovie);

        var item = response.Models[0];
        return new Movie
        {
            Id = item.Id,
            Title = item.Title,
            Category = item.Category,
            WatchedStatus = Enum.Parse<WatchedStatus>(item.WatchedStatus),
            AddedBy = item.AddedBy,
            AddedAt = item.AddedAt
        };
    }
    
    public async Task DeleteMovieAsync(string movieId)
    {
        await _client
            .From<SupabaseMovie>()
            .Where(x => x.Id == movieId)
            .Delete();
    }
}