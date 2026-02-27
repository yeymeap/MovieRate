using System;
using System.Linq;
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
                Members = item.Members,
                CreatedAt = item.CreatedAt,
                IsOwner = item.OwnerId == _authService.CurrentUser?.Id
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
            Members = item.Members,
            CreatedAt = item.CreatedAt,
            IsOwner = true
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
            var userData = await GetUserMovieDataAsync(item.Id);
            movies.Add(new Movie
            {
                Id = item.Id,
                TmdbId = item.TmdbId,
                Title = item.Title,
                PosterUrl = item.PosterUrl,
                Category = item.Category,
                ReleaseDate = item.ReleaseDate,
                AddedBy = item.AddedBy,
                Rating = userData?.Rating ?? 0,
                WatchedStatus = userData != null ? Enum.Parse<WatchedStatus>(userData.WatchedStatus) : WatchedStatus.Unwatched
            });
        }
        return movies;
    }
    
    public async Task<Movie?> AddMovieAsync(string listId, string title, string category, string tmdbId = "", string posterUrl = "", string releaseDate = "")
    {
        var userId = _authService.CurrentUser?.Id ?? string.Empty;
        var newMovie = new SupabaseMovie
        {
            ListId = listId,
            Title = title,
            Category = category,
            TmdbId = tmdbId,
            PosterUrl = posterUrl,
            ReleaseDate = releaseDate,
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
            TmdbId = item.TmdbId,
            PosterUrl = item.PosterUrl,
            ReleaseDate = item.ReleaseDate,
            AddedBy = item.AddedBy
        };
    }
    
    public async Task DeleteMovieAsync(string movieId)
    {
        await _client
            .From<SupabaseMovie>()
            .Where(x => x.Id == movieId)
            .Delete();
    }
    
    public async Task UpdateMovieRatingAsync(string movieId, int rating)
    {
        var userId = _authService.CurrentUser?.Id ?? string.Empty;
        var existing = await GetUserMovieDataAsync(movieId);
        await UpsertUserMovieDataAsync(movieId, rating, existing != null ? Enum.Parse<WatchedStatus>(existing.WatchedStatus) : WatchedStatus.Unwatched);
    }

    public async Task UpdateMovieWatchedStatusAsync(string movieId, WatchedStatus status)
    {
        var existing = await GetUserMovieDataAsync(movieId);
        await UpsertUserMovieDataAsync(movieId, existing?.Rating ?? 0, status);
    }
    
    public async Task<SupabaseProfile?> GetProfileByEmailAsync(string email)
    {
        var response = await _client
            .From<SupabaseProfile>()
            .Where(x => x.Email == email)
            .Single();

        return response;
    }

    public async Task ShareListAsync(string listId, string userId)
    {
        var list = await _client
            .From<SupabaseList>()
            .Where(x => x.Id == listId)
            .Single();

        if (list == null) return;

        list.Members[userId] = "editor";

        await _client
            .From<SupabaseList>()
            .Where(x => x.Id == listId)
            .Set(x => x.Members, list.Members)
            .Update();
    }
    
    public async Task<List<SupabaseProfile>> GetListMembersAsync(string ownerId, Dictionary<string, string> members)
    {
        var ids = new List<string> { ownerId };
        ids.AddRange(members.Keys);

        var response = await _client
            .From<SupabaseProfile>()
            .Filter("id", Postgrest.Constants.Operator.In, ids)
            .Get();

        return response.Models;
    }

    public async Task RemoveMemberAsync(string listId, string userId)
    {
        await _client.Rpc("remove_list_member", new Dictionary<string, object>
        {
            { "list_id", listId },
            { "member_id", userId }
        });
    }
    
    public async Task<string> GetUserEmailAsync(string userId)
    {
        try
        {
            var response = await _client
                .From<SupabaseProfile>()
                .Where(x => x.Id == userId)
                .Single();
            return response?.Email ?? userId;
        }
        catch
        {
            return userId;
        }
    }
    
    public async Task<string> GetDisplayNameAsync(string userId)
    {
        try
        {
            var response = await _client
                .From<SupabaseProfile>()
                .Where(x => x.Id == userId)
                .Single();
            return response?.DisplayName ?? response?.Email ?? userId;
        }
        catch
        {
            return userId;
        }
    }
    
    public async Task<SupabaseMovieUserData?> GetUserMovieDataAsync(string movieId)
    {
        try
        {
            var userId = _authService.CurrentUser?.Id ?? string.Empty;
            return await _client
                .From<SupabaseMovieUserData>()
                .Where(x => x.MovieId == movieId && x.UserId == userId)
                .Single();
        }
        catch
        {
            return null;
        }
    }

    public async Task UpsertUserMovieDataAsync(string movieId, int rating, WatchedStatus watchedStatus)
    {
        var userId = _authService.CurrentUser?.Id ?? string.Empty;
    
        try
        {
            var existing = await GetUserMovieDataAsync(movieId);
            if (existing == null)
            {
                var data = new SupabaseMovieUserData
                {
                    MovieId = movieId,
                    UserId = userId,
                    Rating = rating,
                    WatchedStatus = watchedStatus.ToString()
                };
                await _client
                    .From<SupabaseMovieUserData>()
                    .Insert(data);
            }
            else
            {
                await _client
                    .From<SupabaseMovieUserData>()
                    .Where(x => x.MovieId == movieId && x.UserId == userId)
                    .Set(x => x.Rating, rating)
                    .Set(x => x.WatchedStatus, watchedStatus.ToString())
                    .Update();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpsertUserMovieData failed: {ex.Message}");
        }
    }

    public async Task<List<SupabaseMovieUserData>> GetAllMemberMovieDataAsync(string movieId)
    {
        try
        {
            var response = await _client
                .From<SupabaseMovieUserData>()
                .Where(x => x.MovieId == movieId)
                .Get();
            return response.Models;
        }
        catch
        {
            return new List<SupabaseMovieUserData>();
        }
    }
}