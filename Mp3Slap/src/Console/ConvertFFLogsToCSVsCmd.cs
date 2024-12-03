using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"ffsilence-logs-to-csv",
	description: "Converts ffmpeg silencedetect scripts to beautiful and clean CSV files.",
	Alias = "ff-to-csv")]
public class ConvertFFLogsToCSVsCmd : SilenceDetectShared
{
	[Option("--pad", description: "Amount to pad beginning of audio with", DefVal = FFSilenceTracksParser.PadDefault)]
	public double Pad { get; set; }

	public async Task HandleAsync()
	{
		if(_parseDurationsError != null || Durations.IsNulle()) {
			$"Durations invalid: {_parseDurationsError}".Print();
			return;
		}
		string currDir = ConsoleRun.CurrentDirectory;

		ConvertFFMpegSilenceLogsToCSVArgs args = new() {
			Directory = currDir,
			SilenceDurations = Durations,
			LogFolder = LogFolderName,
			Verbose = Verbose,
			Pad = Pad,
		};

		SResult initRes = args.INIT();
		if(!initRes.Success) {
			$"Config error (wasn't run): {initRes.Message}".Print();
			return;
		}

		RunSilenceDetect runner = new();
		await runner.RUN(RunnerType.ConvertFFMpegSilenceLogsToCSVs, args);

	}
}
