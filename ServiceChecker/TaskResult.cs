using ServiceChecker;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed record TaskResult
{
    public TaskResult(string? name, string? url, long latency, int expectedStatusCode, int actualStatusCode, CheckStatusCodes checkStatus)
    {
        Name = name;
        Url = url;
        Latency = latency;
        ExpectedStatusCode = expectedStatusCode;
        ActualStatusCode = actualStatusCode;
        CheckStatus = checkStatus;
    }

    public TaskResult(TaskDefinition task, long latency, int actualStatusCode, CheckStatusCodes checkStatus)
    {
        Name = task.Name;
        Url = task.Url;
        Latency = latency;
        ExpectedStatusCode = task.ExpectedStatusCode;
        ActualStatusCode = actualStatusCode;
        CheckStatus = checkStatus;
    }

    public string? Name { get; init; }
    public string? Url { get; init; }
    public long Latency { get; init; }
    public int ExpectedStatusCode { get; init; }
    public int ActualStatusCode { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CheckStatusCodes CheckStatus { get; init; }

    public override string ToString()
    {
        if (CheckStatus == CheckStatusCodes.OK || CheckStatus == CheckStatusCodes.UNEXPECTED_STATUS)
            return $"{Name, -25}   {Url,-35}  Ожидался код:{ExpectedStatusCode}   Получен код:{ActualStatusCode}  Статус проверки:{CheckStatus}  Ожидание:{Latency} мс";

        return $"{Name,-25}   {Url,-35}  Статус:{CheckStatus}";
    }

}