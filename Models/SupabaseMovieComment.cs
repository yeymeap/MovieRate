using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MovieRate.Models;

[Table("movie_comments")]
public class SupabaseMovieComment : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("movie_id")]
    public string MovieId { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}