using System;
using Microsoft.Extensions.Configuration;
using MovieRate.Models;

namespace MovieRate.Services;

public static class ConfigService
{
    private static FirebaseConfig? _config;

    public static FirebaseConfig GetFirebaseConfig()
    {
        if (_config != null) return _config;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _config = new FirebaseConfig();
        configuration.GetSection("Firebase").Bind(_config);
        return _config;
    }
}