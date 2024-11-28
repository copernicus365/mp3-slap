using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.Console.SilenceDetect;

[Command(
	"ffsilence-logs-to-csv",
	description: "Converts ffmpeg silencedetect scripts to beautiful and clean CSV files.",
	Alias = "ff-to-csv")]
public class ConvertFFLogsToCSVsCmd : SilenceDetectShared
{
	public async Task HandleAsync()
	{
		string currDir = ConsoleRun.CurrentDirectory;

		SilenceDetectArgs args = new() {
			Directory = currDir,
			SilenceDurations = Durations,
			LogFolder = LogFolderName,
			Verbose = Verbose,
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
