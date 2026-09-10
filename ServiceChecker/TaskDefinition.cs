using System.ComponentModel.DataAnnotations;

public sealed record TaskDefinition
{
    [Required(AllowEmptyStrings = false)]
    public string? Url { get; init; }

    [Range(0, int.MaxValue)]
    public int Retry { get; init; } = 0;

    [Range(1, int.MaxValue)]
    public int Timeout { get; init; } = 1;
}