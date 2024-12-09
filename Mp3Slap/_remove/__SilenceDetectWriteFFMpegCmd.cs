//using System.CommandLine;

//using CommandLine.EasyBuilder;
//using CommandLine.EasyBuilder.Auto;

//using Mp3Slap.SilenceDetect;

//namespace Mp3Slap.CLI.SilenceDetect;

//public class SilenceDetectShared
//{
//	[Option(
//		"--logs-folder-name",
//		"-logname",
//		description: "Name of the folder in which the scripts will be written to. Will have the duration appended to it",
//		DefVal = SilenceDetectArgs.DefaultLogFolder,
//		Required = true)]
//	public string LogFolderName { get; set; }

//	[Option(
//		"--silence-durations",
//		"-dur",
//		"Minimum duration of silence to detect, in seconds, comma separated",
//		Required = true)]
//	public string DurationsStr {
//		get => Durations?.JoinToString(",");
//		set => Durations = ArgParsers.DoubleArray(value, out _parseDurationsError);
//	}

//	protected string _parseDurationsError;

//	public double[] Durations { get; set; }

//	[Option(
//		"--verbose",
//		"-v",
//		description: "Verbose or not.",
//		DefVal = true)]
//	public bool Verbose { get; set; }
//}

//[Command(
//	"write-ffsilence-script",
//	description: "Writes a series of ffmpeg silencedetect scripts, one per detected input audio file, and a single script that can be called to run them all. These will generate ffmpeg's arcane and difficult logs, but other commands here can be used to process those.",
//	Alias = "silenceff")]
//public class SilenceDetectWriteFFMpegCmd : SilenceDetectShared
//{
//	[Option(
//		"--last-name",
//		"-ln",
//		description: "Bogus last name",
//		DefVal = "Trump",
//		Required = true)]
//	public string LastName { get; set; }

//	[Option(
//		"--write-relative-paths",
//		"-rel",
//		"Flag if ffmpeg scripts written should use relative paths.",
//		DefVal = true)]
//	public bool WriteRelativePaths { get; set; }

//	[Option(
//		"--search-pattern",
//		"-s",
//		"Wildcard pattern to match audio files within directory.",
//		DefVal = SilenceDetectWriteFFMpegScriptArgs.AudioFilesSearchPatternDef)]
//	public string AudioFilesSearchPattern { get; set; }

//	[Option(
//		"--include-subdirectories",
//		"-subd",
//		"Flag whether to include subdirectories when search for audio files",
//		DefVal = false)]
//	public bool IncludeSubDirectories { get; set; }

//	public async Task HandleAsync()
//	{
//		string currDir = ConsoleRun.CurrentDirectory;

//		SilenceDetectWriteFFMpegScriptArgs args = new() {
//			Directory = currDir,
//			SilenceDurations = Durations,
//			LogFolder = LogFolderName,
//			WriteRelativePaths = WriteRelativePaths,
//			AudioFilesSearchPattern = AudioFilesSearchPattern,
//			IncludeSubDirectories = IncludeSubDirectories,
//			Verbose = Verbose,
//			// [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];
//		};

//		SResult initRes = args.INIT();
//		if(!initRes.Success) {
//			$"Config error (wasn't run): {initRes.Message}".Print();
//			return;
//		}

//		RunSilenceDetect runner = new();
//		await runner.RUN(RunnerType.WriteFFMpegSilenceScript, args);

//	}
//}
