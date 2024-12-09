using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"to-csv",
	description: "Generate a single silence detections csv file (silencedetect-parsed.csv) from ffmpeg silencedetect log")]
public class SilenceDetectFFLogToCsvSingleCmd
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
		DefVal = FFSilenceTracksParser.PadDefault)]
	public double Pad { get; set; }

	public async Task HandleAsync()
	{
		if(!Src.Exists) {
			"File doesn't exist".Print();
			return;
		}

		string srcPath = Src.FullName;
		string dest = SaveCsvPath
			?? LogFileNames.GetSilenceDetCsvPathFromFFMpegLogPath(srcPath);
		string audMarkersPath = LogFileNames.GetSilenceDetCsvPathFromFFMpegLogPath(srcPath, forAudMarkers: true);

		if(!AllowOverwrite && File.Exists(dest)) {
			"Save path exists and overwrite not allowed".Print();
			return;
		}

		string logCont = File.ReadAllText(srcPath);

		if(logCont.IsNulle() || logCont.Length > 100_000) {
			"Invalid content".Print();
			return;
		}

		WriteFFMpegSilDetLogToCsvs ww = new() {
			Pad = Pad,
			WriteAuditionMarkerCsvs = true,
		};
		await ww.RUN(
			ffLogPath: srcPath,
			csvPath: dest,
			audMarkersPath: audMarkersPath,
			ffLogContent: logCont,
			audioFilePath: null);
	}
}
