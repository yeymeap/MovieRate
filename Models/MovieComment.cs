using System;

namespace MovieRate.Models;

public class MovieComment
{
    public string Id { get; set; } = string.Empty;
    public string MovieId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsOwn { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}