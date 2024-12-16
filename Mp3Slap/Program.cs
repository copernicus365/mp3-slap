using System.CommandLine;
using System.CommandLine.Parsing;

using CommandLine.EasyBuilder;

using Mp3Slap.CLI;
using Mp3Slap.CLI.SilenceDetect;

using static System.Console;

namespace Mp3Slap;

public class Program
{
	public static async Task<int> Main(string[] args)
	{
		INIT();

		RootCommand rootCommand = BuildApp();

		if(args.NotNulle())
			rootCommand.Invoke(args);
		else
			rootCommand.Invoke("-h");

		while(true) {
			Write("\nInput: ");
			string arg = ReadLine();

			int res = arg == null
				? await rootCommand.InvokeAsync(args)
				: await rootCommand.InvokeAsync(arg);
		}
	}

	public static bool IsDebug;

	/// <summary>
	/// This is so clean and simple now, can have it here at root,
	/// leave the cmd specs to their own separate.
	/// </summary>
	public static RootCommand BuildApp()
	{
		RootCommand r = new("mp3 SLAP! Helper lib to ffmpeg and more");

		Command sdGroup = r
			.AddAutoCommand<SilenceDetectGroupCmd>(); // "sd" / "silence-detect"

		sdGroup
			.AddAutoCommand<FullFolderGroupCmd>() // "gr" / "group"
			.Auto<RunFFMpegSilenceDetectOnFolderCmd>() // "run-ff" / "run-ff-full"
			.Auto<ProcessCsvLogsFoldersCmd>() // "csv-logs" / "csvs",
			;

		sdGroup
			.AddAutoCommand<SDCsvGroupCmd>() // "csv" / "sd-csv"
			.Auto<FFLogToCsvSingleCmd>() // "ff-to-csv" / "fflog-to-csv"
			.Auto<ProcessCsvLogCmd>() // "csv-log" / "single"
			;

		r.AddAutoCommand<SetDirectoryCmd>()
			.AddAutoCommand<PrintCurrDirectoryCmd>();

		return r;
	}

	public static string CurrentDirectory;

	public static void INIT()
	{
		IsDebug =
#if DEBUG
	true;
#else
	false;
#endif

		string dir = Environment.CurrentDirectory;

		bool setFromStartupDebugTxt = IsDebug && TrySetRootDirFromStartupDebugTxt(ref dir);

		SetCurrDir(dir, print: true);
	}

	static bool TrySetRootDirFromStartupDebugTxt(ref string dir)
	{
		string path = ProjectPath.BaseDirectoryPath("ignore/startup.txt");
		if(File.Exists(path)) {
			string content = File.ReadAllLines(path)
				.Select(ln => ln.NullIfEmptyTrimmed())
				.Where(ln => ln != null && !ln.StartsWith('#'))
				.FirstOrDefault();

			if(content.InRange(8, 120) && Directory.Exists(content)) {
				dir = content;
				return true;
			}
		}
		return false;
	}

	public static void SetCurrDir(string dir, bool print = true)
	{
		Environment.CurrentDirectory = CurrentDirectory = PathHelper.CleanDirPath(dir);
		if(print)
			WriteLine($"Current directory: {CurrentDirectory}");
	}
}

public static class AutoBuilderFXADD
{
	public static Command Auto<TAuto>(this Command parentCmd) where TAuto : class, new()
	{
		var cmd11 = parentCmd.AddAutoCommand<TAuto>();
		return parentCmd;
	}
}
