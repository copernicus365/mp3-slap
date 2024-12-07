using System.CommandLine;

using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"silence-detect",
	description: "Mega: runs ffmpeg silencedetect scripts, one per detected input audio file, and then immediately converts that output (internally received) to converted CSV silence detect files.",
	Alias = "sd")]
public class MegaSilenceDetectCmd
{
	[Option(
		"--mode",
		"-m",
		"Mode to run",
		DefVal = SilenceDetectMode.fullrun)]
	public SilenceDetectMode Mode { get; set; }

	[Option(
		"--logs-folder-name",
		"-logname",
		description: "Name of the folder in which the scripts will be written to. Will have the duration appended to it",
		DefVal = SilenceDetectArgs.DefaultLogFolder,
		Required = true)]
	public string LogFolderName { get; set; }

	[Option(
		"--silence-durations",
		"-dur",
		"Minimum duration of silence to detect, in seconds, comma separated",
		Required = true)]
	public string DurationsStr {
		get => Durations?.JoinToString(",");
		set => Durations = ArgParsers.DoubleArray(value, out _parseDurationsError);
	}

	protected string _parseDurationsError;

	public double[] Durations { get; set; }

	[Option("--pad",
		description: "Amount to pad beginning of audio with in seconds. The ffmpeg silence detection gives start times precisely when the silence ends / sound begins, but typically you would't want the start of the track to begin without some padding. At the same time most of the long silence occurs at the end of a track.",
		DefVal = FFSilenceTracksParser.PadDefault)]
	public double Pad { get; set; }

	[Option(
		"--verbose",
		"-v",
		description: "Verbose or not.",
		DefVal = true)]
	public bool Verbose { get; set; }

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
		"--write-ffmpeg-logs",
		alias: null,
		"Flag if to write the ffmpeg silencedetect original logs. It is these that are obtained by running ffmpeg, and then converted to the more readable csv results (not needed except for diagnostics)",
		DefVal = false)]
	public bool WriteFFMpegSilenceLogs { get; set; }


	public async Task HandleAsync()
	{
		if(_parseDurationsError != null || Durations.IsNulle()) {
			$"Durations invalid: {_parseDurationsError}".Print();
			return;
		}

		string currDir = Program.CurrentDirectory;

		MegaSilenceDetectArgs args = new() {
			Directory = currDir,
			SilenceDurations = Durations,
			LogFolder = LogFolderName,
			WriteRelativePaths = WriteRelativePaths,
			AudioFilesSearchPattern = AudioFilesSearchPattern,
			IncludeSubDirectories = IncludeSubDirectories,
			Verbose = Verbose,
			Pad = Pad,
			RunFFScript = true,
			WriteFFMpegSilenceLogs = WriteFFMpegSilenceLogs,
		};

		SResult initRes = args.INIT();
		if(!initRes.Success) {
			$"Config error (wasn't run): {initRes.Message}".Print();
			return;
		}

		MegaSilenceDetectScript[] results2 = await MegaSilenceDetectScript.RunManyDurations(
			args,
			async script => {
				switch(Mode) {
					case SilenceDetectMode.fullrun:
						await script.RUN_All();
						break;
					case SilenceDetectMode.audcsv:
					default:
						await script.RUN_Write_AuditionMarkerCSVs_From_SilenceCSVs();
						break;
				}
			});
	}
}

public enum SilenceDetectMode
{
	fullrun,
	audcsv
}
