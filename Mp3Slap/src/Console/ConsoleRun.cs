using System.CommandLine;
using System.CommandLine.Parsing;

using CommandLine.EasyBuilder;

using Mp3Slap.SilenceDetect;

using static System.Console;

namespace Mp3Slap;

public class ConsoleRun
{
	public static string CurrentDirectory = null;

	public static async Task<int> Main(string[] args)
	{
		CurrentDirectory = Environment.CurrentDirectory;

		bool doDiffDir = true;
		if(doDiffDir)
			CurrentDirectory = "C:/Dropbox/Music/Bible/Suchet-NIV-1Album";
		// CurrentDirectory = "C:/Dropbox/Vids/mp3-split/mp3-split/test1"

		CurrentDirectory = PathHelper.CleanDirPath(CurrentDirectory);

		RootCommand rootCommand = BuildConsoleLineApp();

		rootCommand.Invoke("-h");

		while(true) {
			Write("Input: ");
			string arg = ReadLine();

			int res = arg == null
				? await rootCommand.InvokeAsync(args)
				: await rootCommand.InvokeAsync(arg);
		}
	}

	static RootCommand BuildConsoleLineApp()
	{
		RootCommand rootCmd = new("mp3 SLAP! Helper lib to ffmpeg and etc");

		// making these options separate bec used in multiple commands

		Option<string> logFolderName = new Option<string>(
			name: "--logs-folder-name",
			description: "Name of the folder in which the scripts will be written to. If seeking more silence durations than one, typically you'll want this to have {duration} within it")
			.DefaultValue(SilenceDetectArgs.DefaultLogFolder)
			.Alias("-logname");

		Option<double[]> silenceDurOpt = new Option<double[]>(
			name: "--silence-durations",
			parseArgument: ArgParsers.DoubleArray,
			description: "Minimum duration of silence to detect, in seconds, comma separated.")
			.Alias("-d");

		Command silenceDetWriteFFMpeg = new Command(
			name: "write-ffsilence-script",
			description: "Writes a series of ffmpeg silencedetect scripts, one per detected input audio file, and a single script that can be called to run them all. These will generate ffmpeg's arcane and difficult logs, but other commands here can be used to process those.")
			.Init(
				silenceDurOpt,
				logFolderName,
				new Option<bool>(
					name: "--write-relative-paths",
					description: "Flag if ffmpeg scripts written should use relative paths.")
					.DefaultValue(true)
					.Alias("-r"),
				new Option<string>(
					name: "--search-pattern",
					description: "Wildcard pattern to match audio files within directory.")
					.DefaultValue(SilenceDetectWriteFFMpegScriptArgs.AudioFilesSearchPatternDef)
					.Alias("-s"),
				new Option<bool>(
					name: "--include-subdirectories",
					description: "Flag whether to include subdirectories when search for audio files")
					.DefaultValue(false)
					.Alias("-subd"),
				new Option<bool>(
					name: "--verbose",
					description: "Flag if scripts written should have relative paths used.")
					.DefaultValue(true)
					.Alias("-v"),
				handle: async (double[] durations, string logFolderName, bool writeRelPaths, string searchPattern, bool includeSubdirs, bool verbose) => {
					SilenceDetectWriteFFMpegScriptArgs args = new() {
						Directory = CurrentDirectory,
						SilenceDurations = durations,
						LogFolder = logFolderName,
						WriteRelativePaths = writeRelPaths,
						AudioFilesSearchPattern = searchPattern,
						IncludeSubDirectories = includeSubdirs,
						Verbose = verbose,
						// [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];
					};

					SResult initRes = args.INIT();
					if(!initRes.Success) {
						$"Config error (wasn't run): {initRes.Message}".Print();
						return;
					}

					RunSilenceDetect runner = new();
					await runner.RUN(RunnerType.WriteFFMpegSilenceScript, args);
				},
				rootCmd)
				.Alias("silenceff");

		Command convertFFLogsToCSVsCmd = new Command(
			name: "ffsilence-logs-to-csv",
			description: "Converts ffmpeg silencedetect scripts to beautiful and clean CSV files.")
			.Init(
				silenceDurOpt,
				logFolderName,
				new Option<bool>(
					name: "--verbose",
					description: "Flag if scripts written should have relative paths used.")
					.DefaultValue(true)
					.Alias("-v"),
				handle: async (double[] durations, string logFolderName, bool verbose) => {
					SilenceDetectArgs args = new() {
						Directory = CurrentDirectory,
						SilenceDurations = durations,
						LogFolder = logFolderName,
						Verbose = verbose,
					};

					SResult initRes = args.INIT();
					if(!initRes.Success) {
						$"Config error (wasn't run): {initRes.Message}".Print();
						return;
					}

					RunSilenceDetect runner = new();
					await runner.RUN(RunnerType.ConvertFFMpegSilenceLogsToCSVs, args);
				},
				rootCmd)
				.Alias("ff-to-csv");

		/*
		// silencedetect-write-ffmpeg-script
		// ffsilence
		public class SilenceDetectWriteFFMpegScriptArgs : SilenceDetectArgsBase

		// convert-ffmpeg-silence-logs-to-csv
		// tocsv
		public class ConvertFFMpegSilenceLogsToCSVArgs : SilenceDetectArgsBase
		 */

		return rootCmd;
	}
}
