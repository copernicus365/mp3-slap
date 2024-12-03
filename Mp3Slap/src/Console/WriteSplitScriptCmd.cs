using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"write-split-script",
	description: "Writes final splitter script calling ffmpeg to split a single file to albums.",
	Alias = "splitsc")]
public class WriteSplitScriptCmd
{
	[Option("--remove-prefix", description: "Remove prefix from filename to make folder name etc")]
	public string RemovePrefix { get; set; }

	[Option("--extra-dir", description: "Nested directory under current if any")]
	public string ExtrDir { get; set; }

	[Option("--csv-tracks", description: "CSV tracks", Required = true)]
	public FileInfo TracksPath { get; set; }

	[Option("-f", description: "Source (mp3 etc) file path")]
	public FileInfo SrcFile { get; set; }

	public async Task HandleAsync()
	{
		string currDir = ConsoleRun.CurrentDirectory;

		if(TracksPath == null || !TracksPath.Exists)
			return;
		if(SrcFile == null || !SrcFile.Exists)
			return;

		string csvStr = File.ReadAllText(TracksPath.FullName);

		TrackTimeStampsCsv csv = new();
		csv.Parse(csvStr);


		WriteMp3SplitterScript args = new() {
			Dir = currDir,
			SrcFile = SrcFile.FullName,
			ExtrDir = ExtrDir,
			RemovePrefix = RemovePrefix,
		};

		args.INIT();

		args.RUN(csv.Stamps.ToArray());

		//RunSilenceDetect runner = new();
		//await runner.RUN(RunnerType.ConvertFFMpegSilenceLogsToCSVs, args);

	}
}
