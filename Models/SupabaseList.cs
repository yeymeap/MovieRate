using System;
using System.Collections.Generic;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MovieRate.Models;

[Table("lists")]
public class SupabaseList : BaseModel
{
    [PrimaryKey("id")]
    public string Id { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("owner_id")]
    public string OwnerId { get; set; } = string.Empty;

    [Column("members")]
    public Dictionary<string, string> Members { get; set; } = new();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}