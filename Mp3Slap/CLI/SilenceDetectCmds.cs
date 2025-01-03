using System.CommandLine;

using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

public class SilenceDetectBase
{
	[Option(
		"--directory",
		"-d",
		"Root directory to work and search from. Otherwise CD / current directory used",
		Required = false)]
	public DirectoryInfo RootDir { get; set; }


	[Option(
		"--logs-folder-name",
		"-logname",
		"Name of the folder in which the scripts will be written to. Will have the duration appended to it",
		DefVal = SilenceDetectArgs.DefaultLogFolder,
		Required = true)]
	public string LogFolderName { get; set; }

	[Option(
		"--silence-durations",
		"-dur",
		"Minimum duration(s) of silence to detect, in seconds, comma separated",
		Required = true)]
	public string DurationsStr {
		get => Durations?.JoinToString(",");
		set => Durations = ArgParsers.DoubleArray(value, out _parseDurationsError);
	}

	protected string _parseDurationsError;

	public double[] Durations { get; set; }

	[Option(
		"--pad",
		description: "Amount to pad beginning of audio with in seconds. In addition to start/end of sound times, our CSV logs give *calculated* track start / end times that will use this value. If 0, all the silence gap between tracks will be at the END of the track in those calculated times, etc.",
		DefVal = FFSDLogToTimeStampsParser.PadDefault)]
	public double Pad { get; set; }

	[Option(
		"--search-pattern",
		"-s",
		"Wildcard pattern to match audio files within directory.",
		DefVal = SilenceDetectArgs.AudioFilesSearchPatternDef)]
	public string AudioFilesSearchPattern { get; set; }

	[Option(
		"--include-subdirectories",
		"-subd",
		"Flag whether to include subdirectories when search for audio files",
		DefVal = false)]
	public bool IncludeSubDirectories { get; set; }

	[Option(
		"--verbose",
		"-v",
		description: "Verbose or not.",
		DefVal = true)]
	public bool Verbose { get; set; }

	protected bool SetArgs(SilenceDetectFullFolderArgs args)
	{
		if(_parseDurationsError != null || Durations.IsNulle()) {
			$"Durations invalid: {_parseDurationsError}".Print();
			return false;
		}

		string dirPath = RootDir == null
			? Program.CurrentDirectory
			: PathHelper.CleanDirPath(RootDir.FullName);

		if(!Directory.Exists(dirPath)) {
			$"Directory doesn't exist".Print();
			return false;
		}

		args.Directory = dirPath;
		args.SilenceDurations = Durations;
		args.LogFolder = LogFolderName;
		args.AudioFilesSearchPattern = AudioFilesSearchPattern;
		args.IncludeSubDirectories = IncludeSubDirectories;
		args.Verbose = Verbose;
		args.Pad = Pad;

		SResult initRes = args.INIT();
		if(!initRes.Success) {
			$"Config error (wasn't run): {initRes.Message}".Print();
			return false;
		}
		return true;
	}
}
