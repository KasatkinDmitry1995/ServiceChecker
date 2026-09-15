using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed record TaskDefinition : IJsonOnDeserialized
{
    [Required(AllowEmptyStrings = false)]
    public string? Name { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string? Url { get; init; }

    [Range(0, int.MaxValue)]
    public int Retry { get; init; } = 0;

    [Range(1, int.MaxValue)]
    public long Timeout { get; init; } = 1;
    [Range(1, int.MaxValue)]
    public int ExpectedStatusCode { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        var errors = new List<string>();

        var ctx = new ValidationContext(this);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(this, ctx, results, validateAllProperties: true))
            errors.AddRange(results.Select(r => r.ErrorMessage ?? "ошибка"));

        if (errors.Count > 0)
            throw new JsonException(
                $"Некорректная задача '{Name}': {string.Join("; ", errors)}");
    }
}