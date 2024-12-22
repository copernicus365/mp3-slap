using System.CommandLine;
using System.CommandLine.Parsing;

using Mp3Slap.CLI.SilenceDetect;

using static System.Console;

namespace Mp3Slap;

public class Program
{
	public static async Task<int> Main(string[] args)
	{
		INIT();

		RootCommand rootCmd = BuildCLI.BuildApp();

		if(args.NotNulle())
			rootCmd.Invoke(args);
		else
			rootCmd.Invoke("-h");

		while(true) {
			Write("\nInput: ");
			string arg = ReadLine();

			int res = arg == null
				? await rootCmd.InvokeAsync(args)
				: await rootCmd.InvokeAsync(arg);
		}
	}

	public static bool IsDebug;
	public static bool UseStartupOnlyForDebug = false;

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

		bool useStartupConfig =
			(IsDebug || !UseStartupOnlyForDebug) &&
			TrySetRootDirFromStartupDebugTxt(ref dir);

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
