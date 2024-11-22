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

		Option<string> logFolderName = new Option<string>(
			name: "--log-folder-name",
			description: "Name of the folder in which the scripts will be written to. If seeking more silence durations than one, typically you'll want this to have {duration} within it")
			.Alias("-logname");

		Option<double[]> silenceDurOpt = new Option<double[]>(
			name: "--silence-durations",
			parseArgument: ArgParsers.DoubleArray,
			description: "Delay between lines, specified as milliseconds per character in a line.")
			.Alias("-d");

		Command silenceDetWriteFFMpeg = new Command(
			name: "silencedetect-write-ffmpeg-script",
			description: "Writes a series of ffmpeg silencedetect scripts, one per detected input audio file, and a single script that can be called to run them all. These will generate ffmpeg's arcane and difficult logs, but other commands here can be used to process those.")
			.Init(
				logFolderName,
				silenceDurOpt,
				new Option<bool>(
					name: "--write-relative-paths",
					description: "Flag if scripts written should have relative paths used.")
					.Alias("-rel"),
				new Option<string>(
					name: "--audio-files-search",
					description: "File wildcard search for finding audio files to act on within directory.")
					.DefaultValue("*.mp3"),
				new Option<bool?>(
					name: "--verbose",
					description: "Flag if scripts written should have relative paths used."),
				handle: async (string logFolderName, double[] durations, bool writeRelPaths, string audioFilesSearch, bool? verbose) => {

					SilenceDetectWriteFFMpegScriptArgs args = new() {
						Directory = CurrentDirectory,
						LogFolder = logFolderName,
						// [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];
						SilenceDurations = durations,
						WriteRelativePaths = writeRelPaths,
						AudioFilesSearch = audioFilesSearch,
						Verbose = verbose
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
