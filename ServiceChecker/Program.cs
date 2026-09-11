using Microsoft.Extensions.Options;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Threading.Tasks.Sources;


namespace ServiceChecker
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            var inputFilenameOpt = new Option<string>("--input_file")
            {
                Description = "Имя входного файла. По-умолчанию \"links.json\".",
                DefaultValueFactory = parseResult => "links.json",
            };

            var outputFilenameOpt = new Option<string>("--output_file")
            {
                Description = "Имя выходного файла. По-умолчанию \"result.json\".",
                DefaultValueFactory = parseResult => "result.json",
            };

            var maxThreadsOpt = new Option<int>("--max_threads")
            {
                Description = "Число потоков. По-умолчанию 5.",
                DefaultValueFactory = parseResult => 3,
            };

            RootCommand rootCommand = new();
            rootCommand.Options.Add(inputFilenameOpt);
            rootCommand.Options.Add(outputFilenameOpt);
            rootCommand.Options.Add(maxThreadsOpt);

            rootCommand.SetAction(async parseResult =>
            {
                var taskList = TasksLoader.Load(parseResult.GetValue(inputFilenameOpt));
                var servicesChecker = new ServicesChecker(taskList, parseResult.GetValue(maxThreadsOpt), cts.Token);

                try
                {
                    await servicesChecker.Run();
                    cts.Token.ThrowIfCancellationRequested();
                }catch(OperationCanceledException)
                {
                    if (cts.IsCancellationRequested)
                        Environment.ExitCode = (int)ExitCodes.Interrupted;
                }
                finally
                {
                    var resultsSorted = servicesChecker.GetResults().OrderBy(r => r.CheckStatus).ToList();

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    var json = JsonSerializer.Serialize(resultsSorted, options);
                    File.WriteAllText(parseResult.GetValue(outputFilenameOpt), json);

                    Console.Clear();

                    foreach (TaskResult result in resultsSorted)
                        Console.WriteLine(result);

                    var resultCounts = from r in resultsSorted
                                       group r by r.CheckStatus into g
                                       select new { CheckStatus = g.Key, Count = g.Count() };

                    var normalResults = resultsSorted.Where(s => (s.Latency >= 0));
                    long min = normalResults.Min(s => s.Latency);
                    double avg = normalResults.Average(s => s.Latency);
                    long max = normalResults.Max(s => s.Latency);

                    Console.WriteLine();
                    Console.WriteLine($"Всего проанализировано ссылок: {resultsSorted.Count}");
                    Console.WriteLine("Из них:");

                    foreach (var resultCount in resultCounts)
                        Console.WriteLine($"{resultCount.CheckStatus}: {resultCount.Count}");

                    Console.WriteLine();
                    Console.WriteLine("Время доступа");
                    Console.WriteLine($"Минимальное:{min,-5}  Среднее:{avg,-6:F2}  Максимальное: {max,-5}");

                    Console.ReadKey();
                }

            });

            ParseResult parseResult = rootCommand.Parse(args);
            await parseResult.InvokeAsync();

        }
    }
}
