using System.CommandLine;
using System.CommandLine.Parsing;

using CommandLine.EasyBuilder;

namespace Mp3Slap;

public class ConsoleProg
{
	public RootCommand BuildApp()
	{
		RootCommand rootCmd = new("mp3 SLAP! Helper lib to ffmpeg and etc");

		Command quotesCmd = new("quotes", "Work with a file that contains quotes.");
		rootCmd.AddCommand(quotesCmd);

		Option<FileInfo?> fileOption = new Option<FileInfo?>(
			name: "--file",
			description: "An option whose argument is parsed as a FileInfo",
			isDefault: true,
			parseArgument: ParseFileArg) {
			IsRequired = false
		}
			.Alias("-f")
			.DefaultValue("cool.txt");

		rootCmd.AddGlobalOption(fileOption);

		Command readCmd = new Command("read", "Read and display the file.").Init(
			fileOption,
			new Option<int>(
				name: "--delay",
				description: "Delay between lines, specified as milliseconds per character in a line.",
				getDefaultValue: () => 42)
				.Alias("-d"),
			new Option<ConsoleColor>(
				name: "--fgcolor",
				description: "Foreground color of text displayed on the console.",
				getDefaultValue: () => ConsoleColor.White),
			new Option<bool>(
				name: "--light-mode",
				description: "Background color of text displayed on the console: default is black, light mode is white."),
				handle: async (file, delay, fgcolor, lightMode) => {
					$"READING a file (pretend): {file?.FullName}".Print();
					//await ReadFile(file!, delay, fgcolor, lightMode);
				},
				quotesCmd);

		Command deleteCmd = new Command("delete", "Delete lines from the file.").Init(
			fileOption,
			new Option<string[]>(
				name: "--search-terms",
				description: "Strings to search for when deleting entries.") {
				IsRequired = true,
				AllowMultipleArgumentsPerToken = true
			},
			handle: (file, searchTerms) => {
				//DeleteFromFile(file!, searchTerms);
				$"DELETING lines from a file (pretend): {file?.FullName}".Print();
			},
				quotesCmd);

		new Command("add", "Add an entry to the file.").Init(
			fileOption,
			new Argument<string>(
				name: "quote",
				description: "Text of quote."),
			new Argument<string>(
				name: "byline",
				description: "Byline of quote."),
			handle: (file, quote, byline) => {
				//AddToFile(file!, quote, byline);
				$"ADDING lines to a file (pretend): {file?.FullName}".Print();
			},
				quotesCmd)
			.Alias("insert");

		return rootCmd;
	}

	public RootCommand BuildSampleApp()
	{
		RootCommand rootCmd = new("Test console app");

		Command quotesCmd = new("quotes", "Work with a file that contains quotes.");
		rootCmd.AddCommand(quotesCmd);

		Option<FileInfo?> fileOption = new Option<FileInfo?>(
			name: "--file",
			description: "An option whose argument is parsed as a FileInfo",
			isDefault: true,
			parseArgument: ParseFileArg) {
			IsRequired = false
		}
			.Alias("-f")
			.DefaultValue("cool.txt");

		rootCmd.AddGlobalOption(fileOption);

		Command readCmd = new Command("read", "Read and display the file.").Init(
			fileOption,
			new Option<int>(
				name: "--delay",
				description: "Delay between lines, specified as milliseconds per character in a line.",
				getDefaultValue: () => 42)
				.Alias("-d"),
			new Option<ConsoleColor>(
				name: "--fgcolor",
				description: "Foreground color of text displayed on the console.",
				getDefaultValue: () => ConsoleColor.White),
			new Option<bool>(
				name: "--light-mode",
				description: "Background color of text displayed on the console: default is black, light mode is white."),
				handle: async (file, delay, fgcolor, lightMode) => {
					$"READING a file (pretend): {file?.FullName}".Print();
					//await ReadFile(file!, delay, fgcolor, lightMode);
				},
				quotesCmd);

		Command deleteCmd = new Command("delete", "Delete lines from the file.").Init(
			fileOption,
			new Option<string[]>(
				name: "--search-terms",
				description: "Strings to search for when deleting entries.") {
				IsRequired = true,
				AllowMultipleArgumentsPerToken = true
			},
			handle: (file, searchTerms) => {
				//DeleteFromFile(file!, searchTerms);
				$"DELETING lines from a file (pretend): {file?.FullName}".Print();
			},
				quotesCmd);

		new Command("add", "Add an entry to the file.").Init(
			fileOption,
			new Argument<string>(
				name: "quote",
				description: "Text of quote."),
			new Argument<string>(
				name: "byline",
				description: "Byline of quote."),
			handle: (file, quote, byline) => {
				//AddToFile(file!, quote, byline);
				$"ADDING lines to a file (pretend): {file?.FullName}".Print();
			},
				quotesCmd)
			.Alias("insert");

		return rootCmd;
	}

	FileInfo ParseFileArg(ArgumentResult arg)
	{
		string? filePath = arg.Tokens.Count == 0
			? "sampleQuotes.txt"
			: arg.Tokens.Single().Value;

		$"Blah: {filePath}".Print();

		if(!File.Exists(filePath)) {
			arg.ErrorMessage = $"File does not!! exist / '{filePath}'";
			return null;
		}
		return new FileInfo(filePath);
	}

}
