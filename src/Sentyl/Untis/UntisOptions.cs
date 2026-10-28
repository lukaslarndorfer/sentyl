using System.ComponentModel.DataAnnotations;

namespace Sentyl.Untis;

public sealed class UntisOptions
{
    public const string SectionName = "Untis";
    public const string ClientName = "sentyl";

    [Required] public string Host { get; set; } = "";
    [Required] public string School { get; set; } = "";
    [Required] public string User { get; set; } = "";
    [Required] public string Password { get; set; } = "";

}
