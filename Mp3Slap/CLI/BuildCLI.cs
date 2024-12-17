using System.CommandLine;

using CommandLine.EasyBuilder;

namespace Mp3Slap.CLI.SilenceDetect;

public static class BuildCLI
{
	/// <summary>
	/// This is so clean and simple now, can have it here at root,
	/// leave the cmd specs to their own separate.
	/// </summary>
	public static RootCommand BuildApp()
	{
		RootCommand r = new("mp3 SLAP! Helper lib to ffmpeg and more");

		Command sdGroup = r
			.AddAutoCommand<SilenceDetectGroupCmd>(); // "sd" / "silence-detect"

		sdGroup
			.AddAutoCommand<FullFolderGroupCmd>() // "gr" / "group"
			.Auto<RunFFMpegSilenceDetectOnFolderCmd>() // "run-ff" / "run-ff-full"
			.Auto<ProcessCsvLogsFoldersCmd>() // "csv-logs" / "csvs",
			;

		sdGroup
			.AddAutoCommand<SDCsvGroupCmd>() // "csv" / "sd-csv"
			.Auto<FFLogToCsvSingleCmd>() // "ff-to-csv" / "fflog-to-csv"
			.Auto<ProcessCsvLogCmd>() // "csv-log" / "single"
			;

		r.AddAutoCommand<SetDirectoryCmd>()
			.AddAutoCommand<PrintCurrDirectoryCmd>();

		return r;
	}
}
