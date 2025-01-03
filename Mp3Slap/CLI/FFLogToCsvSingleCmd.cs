using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"fflog-to-csv",
	Alias = "ff-to-csv",
	Description = "Generates a single silence detections csv file (silencedetect-parsed.csv) from ffmpeg silencedetect log. Use this when you already have an ffmpeg silence-detect log to process")]
public class FFLogToCsvSingleCmd
{
	[Argument("sd-ff-log",
		description: "Source ffmpeg silencedetect log result output")]
	public FileInfo Src { get; set; }

	[Option("--save-csv-path",
		alias: "-to",
		description: "Path if default not to be used",
		Required = false)]
	public string SaveCsvPath { get; set; }

	[Option("--overwrite",
		alias: "-ov",
		description: "Allow overwrite of destination path",
		DefVal = true)]
	public bool AllowOverwrite { get; set; }

	[Option(
		"--pad",
		description: "Amount to pad beginning of audio with in seconds. The ffmpeg silence detection gives start times precisely when the silence ends / sound begins, but typically you would't want the start of the track to begin without some padding. At the same time most of the long silence occurs at the end of a track.",
		DefVal = FFSDLogToTimeStampsParser.PadDefault)]
	public double Pad { get; set; }

	public async Task HandleAsync()
	{
		if(!Src.Exists) {
			"File doesn't exist".Print();
			return;
		}

		string srcPath = Src.FullName;
		string dest = SaveCsvPath ?? LogFileNames.GetSilenceDetCsvPathFromFFMpegLogPath(srcPath);
		string audMarkersPath = LogFileNames.GetSilenceDetCsvPathFromFFMpegLogPath(srcPath, forAudMarkers: true);

		WriteFFMpegSilDetLogToCsvs ww = new() {
			Pad = Pad,
			WriteAuditionMarkerCsvs = true,
		};

		Mp3ToSplitPathsInfo info = new() {
			FFSDLogPath = srcPath,
			SDTimeStampsCSVPath = dest,
			AudMarkersCSVPath = audMarkersPath,
			AudioFilePath = null,
		};
		await ww.RUN(
			info,
			ffSDLogContent: null,
			allowOverwriteCsv: AllowOverwrite);
			////ffLogPath: srcPath,
			//csvLogPath: dest,
			//audMarkersCsvPath: audMarkersPath,
			//ffLogContent: null,
			//audioFilePath: null,
			//allowOverwriteCsv: AllowOverwrite);
	}
}
