using System.CommandLine;

namespace ServiceChecker
{
    internal static class ProgramHelpers
    {
        private static void AddNotEmptyValidator(Option<string> option)
        {
            option.Validators.Add(result =>
            {
                string? value = result.GetValue(option);
                if (string.IsNullOrWhiteSpace(value))
                {
                    result.AddError($"Значение для {option.Name} не может быть пустым.");
                }
            });
        }

        public static void FillRootCommandParams(out Option<string> inputFilenameOpt, out Option<string> outputFilenameOpt, out Option<int> maxThreadsOpt, out RootCommand rootCommand)
        {
            inputFilenameOpt = new Option<string>("--input_file")
            {
                Description = "Имя входного файла. По-умолчанию \"links.json\".",
                DefaultValueFactory = parseResult => "links.json",
            };
            outputFilenameOpt = new Option<string>("--output_file")
            {
                Description = "Имя выходного файла. По-умолчанию \"result.json\".",
                DefaultValueFactory = parseResult => "result.json",
            };
            maxThreadsOpt = new Option<int>("--max_threads")
            {
                Description = "Число потоков. По-умолчанию 5.",
                DefaultValueFactory = parseResult => 3,
            };
            AddNotEmptyValidator(inputFilenameOpt);
            AddNotEmptyValidator(outputFilenameOpt);

            rootCommand = new();
            rootCommand.Options.Add(inputFilenameOpt);
            rootCommand.Options.Add(outputFilenameOpt);
            rootCommand.Options.Add(maxThreadsOpt);
        }
    }
}