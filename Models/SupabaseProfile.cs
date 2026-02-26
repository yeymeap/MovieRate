using Postgrest.Attributes;
using Postgrest.Models;

namespace MovieRate.Models;

[Table("profiles")]
public class SupabaseProfile : BaseModel
{
    [PrimaryKey("id")]
    public string Id { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;
}