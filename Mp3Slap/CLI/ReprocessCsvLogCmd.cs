using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"csv-log",
	Alias = "single",
	Description = "Reprocesses a single silence detect csv log[1] and possibly (re)generates its Adobe Audition marker csv file ([1] e.g. `log#foo1.mp3#silencedetect-parsed.csv`).")]
public class ReprocessCsvLogCmd
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

	public async Task HandleAsync()
	{
		if(!CsvLog.Exists) {
			"File doesn't exist".Print();
			return;
		}

		string srcPath = CsvLog.FullName;
		string dest = SaveAuditionCsvPath
			?? LogFileNames.GetAuditionMarkersCsvPathFromSilenceCsvPath(srcPath);

		if(!AllowOverwrite && File.Exists(dest)) {
			"Save path exists and overwrite not allowed".Print();
			return;
		}

		string csvCont = File.ReadAllText(srcPath);

		if(csvCont.IsNulle() || csvCont.Length > 50_000) {
			"Invalid content".Print();
			return;
		}

		WriteAuditionMarkerCSVs wcsv = new();
		StampsCsvGroup g = await wcsv.RUN(dest, csvCont);

		//g.stamps
	}
}


