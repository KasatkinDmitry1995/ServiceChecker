using System.Text.Json;
using System.Text.Json.Serialization;

public static class TasksLoader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static IReadOnlyList<TaskDefinition> Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Файл задач не найден: {path}");

        var json = File.ReadAllText(path);

        try
        {
            var tasks = JsonSerializer.Deserialize<List<TaskDefinition>>(json, Options);

            if (tasks is null || tasks.Count == 0)
                throw new JsonException($"Файл {path} пуст или не содержит задач.");

            return tasks;
        }
        catch (JsonException ex)
        {
            throw new JsonException(
                $"Ошибка в {path} (строка {ex.LineNumber}, позиция {ex.BytePositionInLine}): {ex.Message}",
                ex);
        }
    }
}