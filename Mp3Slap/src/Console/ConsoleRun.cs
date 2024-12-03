using System.CommandLine;
using System.CommandLine.Parsing;

using CommandLine.EasyBuilder;

using Mp3Slap.CLI.SilenceDetect;

using static System.Console;

namespace Mp3Slap.CLI;

public class ConsoleRun
{
	public static string CurrentDirectory;

	public static bool IsDebug =
#if DEBUG
	true;
#else
	false;
#endif

	public static async Task<int> Main(string[] args)
	{
		SetCurrDirStartup(args);

		RootCommand rootCommand =
			//OldFunctionalBuildApp.BuildApp(CurrentDirectory)
			BuildApp();

		if(args.NotNulle())
			rootCommand.Invoke(args);
		else
			rootCommand.Invoke("-h");

		while(true) {
			Write("Input: ");
			string arg = ReadLine();

			int res = arg == null
				? await rootCommand.InvokeAsync(args)
				: await rootCommand.InvokeAsync(arg);
		}
	}

	/// <summary>
	/// This is so clean and simple now, can have it here at root,
	/// leave the cmd specs to their own separate.
	/// </summary>
	public static RootCommand BuildApp()
	{
		RootCommand r = new("mp3 SLAP! Helper lib to ffmpeg and etc");

		r.AddAutoCommand<SetDirectoryCmd>()
			.AddAutoCommand<PrintCurrDirectoryCmd>(); // adds print sub-cmd to first returned cd cmd

		r.AddAutoCommand<MegaSilenceDetectCmd>();

		r.AddAutoCommand<SilenceDetectWriteFFMpegCmd>();

		r.AddAutoCommand<ConvertFFLogsToCSVsCmd>();

		r.AddAutoCommand<WriteSplitScriptCmd>();

		r.AddAutoCommand<Test1Cmd>();

		return r;
	}

	public static void SetCurrDirStartup(string[] args)
	{
		string dir = Environment.CurrentDirectory;

		if(IsDebug && args.IsNulle()) {
			string path = ProjectPath.ProjPath("ignore/startup-debug-path.txt");
			if(File.Exists(path)) {
				string content = File.ReadAllLines(path)
					.Select(ln => ln.NullIfEmptyTrimmed())
					.Where(ln => ln != null && !ln.StartsWith('#'))
					.FirstOrDefault();

				if(content.InRange(8, 120) && Directory.Exists(content))
					dir = content;
			}
		}
		SetCurrDir(dir, print: true);
	}

	public static void SetCurrDir(string dir, bool print = true)
	{
		Environment.CurrentDirectory = CurrentDirectory = PathHelper.CleanDirPath(dir);
		if(print)
			WriteLine($"Current directory: {CurrentDirectory}");
	}
}
