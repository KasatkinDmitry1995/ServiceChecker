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

            Option<string> inputFilenameOpt, outputFilenameOpt;
            Option<int> maxThreadsOpt;
            RootCommand rootCommand;
            ProgramHelpers.FillRootCommandParams(out inputFilenameOpt, out outputFilenameOpt, out maxThreadsOpt, out rootCommand);

            rootCommand.SetAction(async (parseResult, cancellationToken) =>
            {
                ServicesChecker? servicesChecker = null;

                try
                {
                    var taskList = TasksLoader.Load(parseResult.GetValue(inputFilenameOpt)!);
                    servicesChecker = new ServicesChecker(taskList, parseResult.GetValue(maxThreadsOpt), cancellationToken);
                    await servicesChecker.Run();
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    Environment.ExitCode = (int)ExitCodes.Interrupted;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Файл не найден.");
                    Environment.ExitCode = (int)ExitCodes.FileNotFound;
                }
                catch(JsonException ex)
                {
                    Console.WriteLine(ex.Message);
                    Environment.ExitCode = (int)ExitCodes.ValidationError;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                    Environment.ExitCode = (int)ExitCodes.GeneralError;
                }
                finally
                {
                    if (servicesChecker != null)
                    {
                        ResultReporter.ReportResult(parseResult, outputFilenameOpt, servicesChecker);
                    }

                    Console.ReadKey();
                }

            });

            
            ParseResult parseResult = rootCommand.Parse(args);

            if (parseResult.Errors.Count > 0)
            {

                foreach (ParseError parseError in parseResult.Errors)
                {
                    Console.Error.WriteLine(parseError.Message);
                }

                Environment.ExitCode = (int)ExitCodes.UsageError;
                return;
            }

            await parseResult.InvokeAsync();
        }
    }
}
