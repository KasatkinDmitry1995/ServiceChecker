using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace ServiceChecker
{
    internal class Program
    {
        static int Main(string[] args)
        {

            var configuration = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

            var tasksPath = configuration["tasks"];

            if (string.IsNullOrWhiteSpace(tasksPath))
            {
                Console.Error.WriteLine("Укажите файл задач: --config <path>");
                return (int)ExitCodes.UsageError;
            }

            if (!File.Exists(tasksPath))
            {
                Console.Error.WriteLine($"Файл задач не найден: {tasksPath}");
                return (int)ExitCodes.FileNotFound;
            }

            Console.WriteLine("Hello, World!");

            return (int)ExitCodes.Success;
        }
    }
}
