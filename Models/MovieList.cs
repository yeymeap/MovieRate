using System;
using System.Collections.Generic;

namespace MovieRate.Models;

public class MovieList
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public Dictionary<string, string> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}