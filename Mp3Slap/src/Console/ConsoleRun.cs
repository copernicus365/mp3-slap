using System.CommandLine;
using System.CommandLine.Parsing;

using CommandLine.EasyBuilder;

using Mp3Slap.Console;
using Mp3Slap.Console.SilenceDetect;

using static System.Console;

namespace Mp3Slap;

public class ConsoleRun
{
	public static string CurrentDirectory;

	public static bool IsDebug = true; // Environment.GetEnvironmentVariable("DEBUG") == "true";

	public static async Task<int> Main(string[] args)
	{
		SetCurrDir(IsDebug && args.IsNulle()
			? "C:/Dropbox/Music/Bible/Suchet-NIV-1Album" //"C:/Dropbox/Vids/mp3-split/mp3-split/test1"
			: Environment.CurrentDirectory, print: true);

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
		RootCommand rootCmd = new("mp3 SLAP! Helper lib to ffmpeg and etc");

		rootCmd.AddAutoCommand<SetDirectoryCmd>();

		rootCmd.AddAutoCommand<SilenceDetectWriteFFMpegCmd>();

		return rootCmd;
	}

	public static void SetCurrDir(string dir, bool print = true)
	{
		CurrentDirectory = PathHelper.CleanDirPath(dir);
		if(print)
			WriteLine($"Current directory: {CurrentDirectory}");
	}
}
