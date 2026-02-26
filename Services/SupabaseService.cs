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
}