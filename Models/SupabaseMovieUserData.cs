using Postgrest.Attributes;
using Postgrest.Models;

namespace MovieRate.Models;

[Table("movie_user_data")]
public class SupabaseMovieUserData : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("movie_id")]
    public string MovieId { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("rating")]
    public int Rating { get; set; }

    [Column("watched_status")]
    public string WatchedStatus { get; set; } = "Unwatched";
}