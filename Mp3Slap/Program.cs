using System.CommandLine;

using Mp3Slap.CLI.SilenceDetect;

using static System.Console;

namespace Mp3Slap;

public class Program
{
	public static async Task<int> Main(string[] args)
	{
		INIT();

		RootCommand rootCmd = BuildCLI.BuildApp();

		bool hasArgs = args.NotNulle();
		bool doLoop = !hasArgs;
		string cmdLine = doLoop ? "-h" : null;
		bool forLoopDoHelpFirstRun = true;

		do {
			if(!hasArgs) {
				if(forLoopDoHelpFirstRun) {
					cmdLine = "-h";
					forLoopDoHelpFirstRun = false;
				}
				else {
					Write("> ");
					cmdLine = ReadLine();
					WriteLine();
				}
			}

			ParseResult res = hasArgs
				? rootCmd.Parse(args)
				: rootCmd.Parse(cmdLine);

			args = null;
			hasArgs = false;

			if(res.Errors.Any()) {
				foreach(var err in res.Errors) {
					WriteLine($"Error: {err.Message}");
				}
				continue;
			}

			int dRes = await res.InvokeAsync();
			WriteLine();
		} while(doLoop);
		return 0;
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
		string path = ProjectPath.BaseDirectoryPath("startup.txt");
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
