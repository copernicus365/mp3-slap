using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"csv",
	Alias = "process-single-sd-csv",
	Description = "For processing a single silence detect csv. Handles removing cuts (soon: + adds), (re)generating Audition marker csv from it, etc ([1] e.g. `log#foo1.mp3#silencedetect-parsed.csv`).")]
public class ProcessCsvLogCmd
{
	[Argument("sd-csv-path",
		description: "Path of the silencedetect csv. Note that when auto-generated, path ends with: `...#silencedetect-parsed.csv`)")]
	public FileInfo CsvLog { get; set; }

	[Option("--resave-csv",
		alias: "-resave",
		description: "True to have (or allow) input csv to be resaved. If false, note that processed cuts or adds won't be saved",
		DefVal = true)]
	public bool ResaveCsvLog { get; set; }

	public async Task HandleAsync()
	{
		if(!CsvLog.Exists) {
			"File doesn't exist".Print();
			return;
		}

		string srcCsvLogPath = CsvLog.FullName;
		string destAudCsvPath = //SaveAuditionCsvPath ??
			LogFileNames.GetAuditionMarkersCsvPathFromSilenceCsvPath(srcCsvLogPath);

		//if(!AllowOverwrite && File.Exists(destAudCsvPath)) {
		//	"Save path exists and overwrite not allowed".Print();
		//	return;
		//}

		string csvCont = File.ReadAllText(srcCsvLogPath);

		if(csvCont.IsNulle() || csvCont.Length > 50_000) {
			"Invalid content".Print();
			return;
		}
		 
		ProcessSilDetCSV wcsv = new() {
			ResaveCsvLogOnChange = ResaveCsvLog,
		};
		SDStampsCsvProcessResult g = await wcsv.RUN(
			destAudCsvPath,
			silenceDetCsvLog: csvCont,
			silenceDetCsvLogPath: srcCsvLogPath);
	}
}

// FOR NOW: Let's simplify...
//[Option("--save-aud-csv-path",
//	alias: "-to",
//	description: "Path if default not to be used",
//	Required = false)]
//public string SaveAuditionCsvPath { get; set; }

//[Option("--overwrite",
//	alias: "-ov",
//	description: "Allow overwrite of destination path",
//	DefVal = true)]
//public bool AllowOverwrite { get; set; }
