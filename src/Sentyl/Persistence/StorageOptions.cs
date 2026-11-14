using System.ComponentModel.DataAnnotations;

namespace Sentyl.Persistence;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    [Required] public string DatabasePath { get; set; } = "";
}
