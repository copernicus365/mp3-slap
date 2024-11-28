using System.CommandLine;

using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.Console;

public class BuildCommandLineApp
{
	public static RootCommand BuildApp()
	{
		RootCommand rootCmd = new("mp3 SLAP! Helper lib to ffmpeg and etc");

		rootCmd.AddAutoCommand<SilenceDetectWriteFFMpegArgs1>();

		return rootCmd;
	}
}

[Command(
	"write-ffsilence-script",
	description: "Writes a series of ffmpeg silencedetect scripts, one per detected input audio file, and a single script that can be called to run them all. These will generate ffmpeg's arcane and difficult logs, but other commands here can be used to process those.",
	Alias = "silenceff")]
public class SilenceDetectWriteFFMpegArgs1
{
	[Option(
		"--logs-folder-name",
		"-logname",
		description: "Name of the folder in which the scripts will be written to. If seeking more silence durations than one, typically you'll want this to have {duration} within it",
		DefVal = SilenceDetectArgs.DefaultLogFolder)]
	public string LogFolderName { get; set; }

	[Option(
		"--current-directory",
		"-d",
		"Sets active current directory")]
	public string CurrentDirectory { get; set; }

	[Option(
		"--silence-durations",
		"-dur",
		"Minimum duration of silence to detect, in seconds, comma separated")]
	public string DurationsStr {
		get => Durations?.JoinToString(",");
		set => Durations = ArgParsers.DoubleArray(value, out string err);
	}

	public double[] Durations { get; set; }

	[Option(
		"--write-relative-paths",
		"-rel",
		"Flag if ffmpeg scripts written should use relative paths.",
		DefVal = true)]
	public bool WriteRelativePaths { get; set; }

	[Option(
		"--search-pattern",
		"-s",
		"Wildcard pattern to match audio files within directory.",
		DefVal = SilenceDetectWriteFFMpegScriptArgs.AudioFilesSearchPatternDef)]
	public string AudioFilesSearchPattern { get; set; }

	[Option(
		"--include-subdirectories",
		"-subd",
		"Flag whether to include subdirectories when search for audio files",
		DefVal = false)]
	public bool IncludeSubDirectories { get; set; }

	[Option(
		"--verbose",
		description: "Verbose or not.",
		DefVal = true)]
	public bool Verbose { get; set; }

	public async Task HandleAsync()
	{
		await Task.Delay(2100);

		if(CurrentDirectory.IsNulle())
			CurrentDirectory = ConsoleRun.CurrentDirectory;

		SilenceDetectWriteFFMpegScriptArgs args = new() {
			Directory = CurrentDirectory,
			SilenceDurations = Durations,
			LogFolder = LogFolderName,
			WriteRelativePaths = WriteRelativePaths,
			AudioFilesSearchPattern = AudioFilesSearchPattern,
			IncludeSubDirectories = IncludeSubDirectories,
			Verbose = Verbose,
			// [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];
		};

		SResult initRes = args.INIT();
		if(!initRes.Success) {
			$"Config error (wasn't run): {initRes.Message}".Print();
			return;
		}

		RunSilenceDetect runner = new();
		await runner.RUN(RunnerType.WriteFFMpegSilenceScript, args);

	}
}
