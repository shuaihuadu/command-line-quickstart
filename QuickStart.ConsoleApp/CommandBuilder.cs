namespace QuickStart.ConsoleApp;


public class CommandBuilder
{


    public static Command BuildRootCommand()
    {
        RootCommand rootCommand = new("Sample app for System.CommandLine");

        Option<FileInfo?> fileOption = new(name: "--file", description: "The file to read and display on the console.");

        rootCommand.AddOption(fileOption);

        rootCommand.SetHandler((file) =>
        {
            ReadFile(file!);
        }, fileOption);

        return rootCommand;
    }

    public static Command BuildReadCommand()
    {
        RootCommand rootCommand = new("Sample app for System.CommandLine");

        Option<FileInfo?> fileOption = new(name: "--file", description: "The file to read and display on the console.");

        Option<int> delayOption = new(
            name: "--delay",
            description: "Delay between lines, specified as milliseconds per character in a line.",
            getDefaultValue: () => 42);

        Option<ConsoleColor> foregroundColorOption = new(
            name: "--fgcolor",
            description: "Foreground color of text display on the console.",
            getDefaultValue: () => ConsoleColor.White);

        Option<bool> lightModeOption = new(
            name: "--light-mode",
            description: "Background color of text displayed on the console: default is black, light mode is white.");

        Command readCommand = new("read", "Read and display the file.")
        {
            fileOption,
            delayOption,
            foregroundColorOption,
            lightModeOption
        };

        readCommand.SetHandler(async (file, delay, foregroundColor, lightMode) =>
        {
            await ReadFileAsync(file!, delay, foregroundColor, lightMode);
        },
        fileOption, delayOption, foregroundColorOption, lightModeOption);

        rootCommand.AddCommand(readCommand);

        return rootCommand;
    }

    public static Command BuildQuotesCommand()
    {
        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "An option whose argument is parsed as a FileInfo",
            isDefault: true,
            parseArgument: result =>
            {
                if (result.Tokens.Count == 0)
                {
                    return new FileInfo("sampleQuotes.txt");
                }

                string? filePath = result.Tokens.Single().Value;
                if (!File.Exists(filePath))
                {
                    result.ErrorMessage = "File does not exists";
                    return null;
                }
                else
                {
                    return new FileInfo(filePath);
                }
            });

        var delayOption = new Option<int>(
            name: "--delay",
            description: "Delay between lines, specified as milliseconds per character in a line.",
            getDefaultValue: () => 42);

        var foregroundColorOption = new Option<ConsoleColor>(
            name: "--fgcolor",
            description: "Foreground color of text display on the console.",
            getDefaultValue: () => ConsoleColor.White);

        var lightModeOption = new Option<bool>(
            name: "--light-mode",
            description: "Background color of text displayed on the console: default is black, light mode is white.");

        var searchTermsOption = new Option<string[]>(
            name: "--search-terms",
            description: "Strings to search for when deleting entries.")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true,
        };

        var quoteArgument = new Argument<string>(
            name: "quote",
            description: "Text of quote.");

        var bylineArgument = new Argument<string>(
            name: "byline",
            description: "Byline of quote.");

        RootCommand rootCommand = new("Sample app for System.CommandLine");
        rootCommand.AddGlobalOption(fileOption);

        var quoteCommand = new Command("quotes", "Work with a file that contains quotes.");
        rootCommand.AddCommand(quoteCommand);

        var readCommand = new Command("read", "Read and display the file")
        {
            delayOption,
            foregroundColorOption,
            lightModeOption
        };
        quoteCommand.AddCommand(readCommand);

        var deleteCommand = new Command("delete", "Delete lines from the file.");
        deleteCommand.AddOption(searchTermsOption);
        quoteCommand.AddCommand(deleteCommand);

        var addCommand = new Command("add", "Add an entry to the file.");
        addCommand.AddArgument(quoteArgument);
        addCommand.AddArgument(bylineArgument);
        addCommand.AddAlias("insert");
        quoteCommand.AddCommand(addCommand);

        readCommand.SetHandler(async (file, delay, fgColor, lightMode) =>
        {
            await ReadFileAsync(file!, delay, fgColor, lightMode);
        },
        fileOption, delayOption, foregroundColorOption, lightModeOption);

        deleteCommand.SetHandler((file, searchTerms) =>
        {
            DeleteFromFile(file!, searchTerms);
        },
        fileOption, searchTermsOption);

        addCommand.SetHandler((file, quote, userId) =>
        {
            AddToFile(file!, quote, userId);
        },
        fileOption, quoteArgument, bylineArgument);

        return rootCommand;
    }

    static void ReadFile(FileInfo file)
    {
        File.ReadLines(file.FullName, Encoding.UTF8).ToList()
            .ForEach(line => Console.WriteLine(line));
    }

    static async Task ReadFileAsync(FileInfo file, int delay, ConsoleColor foregroundColor, bool lightMode)
    {
        Console.BackgroundColor = lightMode ? ConsoleColor.White : ConsoleColor.Black;

        Console.ForegroundColor = foregroundColor;

        List<string> lines = File.ReadLines(file.FullName).ToList();

        foreach (string line in lines)
        {
            Console.WriteLine(line);
            await Task.Delay(delay * line.Length);
        };
    }

    static void DeleteFromFile(FileInfo file, string[] searchTerms)
    {
        Console.WriteLine("Deleting from file");
        File.WriteAllLines(file.FullName, File.ReadLines(file.FullName).Where(line => searchTerms.All(s => !line.Contains(s))).ToList());
    }

    static void AddToFile(FileInfo file, string quote, string byline)
    {
        Console.WriteLine("Adding to file");

        using StreamWriter? writer = file.AppendText();

        writer.WriteLine($"{Environment.NewLine}{Environment.NewLine}{quote}");
        writer.WriteLine($"{Environment.NewLine}-{byline}");

        writer.Flush();
    }
}