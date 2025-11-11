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

			.AddAutoCommand<RunFFMpegSilenceDetectOnFolderCmd>(); // "run-ff" / "run-ff-full"

		Command csvLogsCmd = sdGroup
			.AddAutoCommand<ProcessCsvLogsFoldersCmd>() // "csvs" / "process-sd-csv-group"
			;

		csvLogsCmd
			.AddAutoCommand<ProcessCsvLogCmd>() // "single" / "process-single-sd-csv"
			.AddAutoCommand<FFLogToCsvSingleCmd>() // "ff-to-csv" / "fflog-to-csv"
			;

		r.AddAutoCommand<SetDirectoryCmd>()
			.AddAutoCommand<PrintCurrDirectoryCmd>();

		return r;
	}
}
