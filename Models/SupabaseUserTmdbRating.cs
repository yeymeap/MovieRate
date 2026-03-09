using Postgrest.Attributes;
using Postgrest.Models;

namespace MovieRate.Models;

[Table("user_tmdb_ratings")]
public class SupabaseUserTmdbRating : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("tmdb_id")]
    public string TmdbId { get; set; } = string.Empty;

    [Column("rating")]
    public int Rating { get; set; }

    [Column("watched_status")]
    public string WatchedStatus { get; set; } = "Unwatched";
}