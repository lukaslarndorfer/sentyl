using System.Text.Json.Serialization;

namespace Sentyl.Untis.Models;

public sealed record Teacher(int Id,
    [property: JsonPropertyName("foreName")] string FirstName,
    [property: JsonPropertyName("longName")] string LastName,
    [property: JsonPropertyName("name")] string Abbreviation
    );
