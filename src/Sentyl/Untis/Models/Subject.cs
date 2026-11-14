using System.Text.Json.Serialization;

namespace Sentyl.Untis.Models;

public sealed record Subject(int Id,
    [property: JsonPropertyName("name")] string Abbreviation,
    string LongName,
    // hex values, optional
    string? ForeColor,
    string? BackColor
    );
