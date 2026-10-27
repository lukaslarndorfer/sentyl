using System.ComponentModel.DataAnnotations;

namespace Sentyl;

public sealed class PollingOptions
{
    public const string SectionName = "Polling";

    [Range(typeof(TimeSpan), "00:01:00", "1.00:00:00")]
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);

    [Range(1, 30)]
    public int DaysToFetch { get; set; } = 7;
}
