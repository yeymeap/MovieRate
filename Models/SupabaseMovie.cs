using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MovieRate.Models;

[Table("movies")]
public class SupabaseMovie : BaseModel
{
    [PrimaryKey("id")]
    public string Id { get; set; } = string.Empty;

    [Column("list_id")]
    public string ListId { get; set; } = string.Empty;

    [Column("tmdb_id")]
    public string TmdbId { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("poster_url")]
    public string PosterUrl { get; set; } = string.Empty;

    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Column("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;

    [Column("added_by")]
    public string AddedBy { get; set; } = string.Empty;

    [Column("added_at")]
    public DateTimeOffset AddedAt { get; set; }
}