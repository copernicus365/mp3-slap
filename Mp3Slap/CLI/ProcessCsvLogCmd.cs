using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"csv-log",
	Alias = "single",
	Description = "Processes a single silence detect csv log[1] and possibly (re)generates its Adobe Audition marker csv file ([1] e.g. `log#foo1.mp3#silencedetect-parsed.csv`).")]
public class ProcessCsvLogCmd
{
	[Argument("csv-log",
		description: "Path of the silencedetect csv log path (e.g. `log#foo1.mp3#silencedetect-parsed.csv`) to generate an audition markers csv from")]
	public FileInfo CsvLog { get; set; }

	[Option("--save-aud-csv-path",
		alias: "-to",
		description: "Path if default not to be used",
		Required = false)]
	public string SaveAuditionCsvPath { get; set; }

	[Option("--overwrite",
		alias: "-ov",
		description: "Allow overwrite of destination path",
		DefVal = true)]
	public bool AllowOverwrite { get; set; }

	[Option("--resave-csv-log",
		alias: "-resave",
		description: "True to have or allow input csv log to be resaved (always, or if cuts were fixed)",
		DefVal = true)]
	public bool ResaveCsvLog { get; set; }

	public async Task HandleAsync()
	{
		if(!CsvLog.Exists) {
			"File doesn't exist".Print();
			return;
		}

		string srcCsvLogPath = CsvLog.FullName;
		string destAudCsvPath = SaveAuditionCsvPath
			?? LogFileNames.GetAuditionMarkersCsvPathFromSilenceCsvPath(srcCsvLogPath);

		if(!AllowOverwrite && File.Exists(destAudCsvPath)) {
			"Save path exists and overwrite not allowed".Print();
			return;
		}

		string csvCont = File.ReadAllText(srcCsvLogPath);

		if(csvCont.IsNulle() || csvCont.Length > 50_000) {
			"Invalid content".Print();
			return;
		}

		WriteAuditionMarkerCSVs wcsv = new() {
			ResaveCsvLogOnChange = ResaveCsvLog,
		};
		StampsCsvGroup g = await wcsv.RUN(
			destAudCsvPath,
			silenceDetCsvLog: csvCont,
			silenceDetCsvLogPath: srcCsvLogPath);
	}
}


