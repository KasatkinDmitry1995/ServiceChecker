using System.CommandLine;
using System.Text.Json;

namespace ServiceChecker
{
    internal static class ResultReporter
    {

        public static void ReportResult(ParseResult parseResult, Option<string> outputFilenameOpt, ServicesChecker servicesChecker)
        {
            var resultsSorted = servicesChecker.GetResults().OrderBy(r => r.CheckStatus).ToList();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(resultsSorted, options);
            File.WriteAllText(parseResult.GetValue(outputFilenameOpt)!, json);

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
        }
    }
}