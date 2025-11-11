using System.CommandLine;

using CommandLine.EasyBuilder;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command("run-ff-full",
	Alias = "run-ff",
	Description = "Runs ffmpeg silencedetect scripts, OR at least generates scripts that call ffmpeg to do the same, one per detected input audio file, converting ff's arcane output to CSV files, etc etc")]
public class RunFFMpegSilenceDetectOnFolderCmd : SilenceDetectBase
{
	[Option(
		"--only-write-ffmpeg-scripts",
		"-scripts-only",
		description: "True to NOT run the ffmpeg scripts internally, but only generate the scripts that one can run manually. IF FALSE, no csv conversion will happen of course.",
		DefVal = false)]
	public bool OnlyWriteFFMpegScripts { get; set; }

	[Option(
		"--audition-marker-csvs",
		alias: "-audcsv",
		"True to write audition marker csvs",
		DefVal = true)]
	public bool WriteAuditionMarkerCsvs { get; set; }

	[Option(
		"--write-ffmpeg-output-logs",
		alias: "-fflogs",
		"True to write the ffmpeg silencedetect output to log files (IGNORED if `-ff-only` is false). It is these that are obtained by running ffmpeg, and then converted to the more readable csv results. Helpful for diagnostic purposes, otherwise not needed",
		DefVal = true)]
	public bool WriteFFMpegSilenceLogs { get; set; }

	[Option(
		"--write-relative-paths",
		"-rel",
		"Flag if ffmpeg scripts written should use relative paths.",
		DefVal = true)]
	public bool WriteRelativePaths { get; set; }

	public async Task HandleAsync()
	{
		SilenceDetectFullFolderArgs args = new() {
			RunFFScript = !OnlyWriteFFMpegScripts,
			WriteFFMpegSilenceLogs = WriteFFMpegSilenceLogs,
			WriteRelativePaths = WriteRelativePaths,
			WriteAuditionMarkerCsvs = WriteAuditionMarkerCsvs,
		};

		if(!SetArgs(args))
			return;

		SilenceDetectFullFolderScript[] res = await SilenceDetectFullFolderScript.RunManyDurations(
			args,
			async script => await script.RUN_ALL());
	}
}
