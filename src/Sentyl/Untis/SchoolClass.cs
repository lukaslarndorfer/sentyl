using System.Text.Json.Serialization;

namespace Sentyl.Untis;

public sealed record SchoolClass(int Id,
    string Name,
    [property: JsonPropertyName("teacher1")] int? HomeroomTeacherId
    );
