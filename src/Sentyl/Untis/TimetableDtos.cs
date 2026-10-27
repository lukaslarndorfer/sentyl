using System.Text.Json.Serialization;

namespace Sentyl.Untis;

public sealed record TimetableParams(TimetableOptions Options);

public sealed record TimetableOptions(TimetableElement Element, int StartDate, int EndDate);

// whose timetable to get
public sealed record TimetableElement(int Id, int Type);

// one lesson
public sealed record TimetableEntry(int Id,
int Date, // raw value: yyyyMMdd
int StartTime, // raw time value: hhmm
int EndTime,
string? Code, // e.g. cancelled, irregular; missing for regular lessons
string? ActivityType,
[property: JsonPropertyName("kl")] List<ElementRef> Classes,
[property: JsonPropertyName("te")] List<ElementRef> Teachers,
[property: JsonPropertyName("su")] List<ElementRef> Subjects,
[property: JsonPropertyName("ro")] List<ElementRef> Rooms
);

public sealed record ElementRef(int Id, int? OrgId); // OrgId is the original element in case of substitution
