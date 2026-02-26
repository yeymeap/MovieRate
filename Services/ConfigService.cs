using System;
using Microsoft.Extensions.Configuration;
using MovieRate.Models;

namespace MovieRate.Services;

public static class ConfigService
{
    private static SupabaseConfig? _config;
    private static TmdbConfig? _tmdbConfig;

    public static SupabaseConfig GetSupabaseConfig()
    {
        if (_config != null) return _config;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _config = new SupabaseConfig();
        configuration.GetSection("Supabase").Bind(_config);
        return _config;
    }
    
    public static TmdbConfig GetTmdbConfig()
    {
        if (_tmdbConfig != null) return _tmdbConfig;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _tmdbConfig = new TmdbConfig();
        configuration.GetSection("Tmdb").Bind(_tmdbConfig);
        return _tmdbConfig;
    }
}