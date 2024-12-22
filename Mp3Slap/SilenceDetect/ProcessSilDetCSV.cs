namespace Mp3Slap.SilenceDetect;

public class ProcessSilDetCSV
{
	public bool SaveCsvLog { get; set; } = true;

	public bool SaveCsvLogOnlyOnChange { get; set; }

	public bool SaveAuditionCsv { get; set; } = true;

	public bool NoErrorOnCsvNotExist { get; set; }

	public bool IgnoreAllWOutAddsOrPluses { get; set; }

	public bool HadAddsOrPluses { get; private set; }


	public List<SDStampsCsvProcessResult> Items = [];

	public string ChapterNamePrefix { get; set; } = "Ch ";



	public async Task RUN(List<Mp3ToSplitPathsInfo> infos)
	{
		if(infos.IsNulle()) return;

		NoErrorOnCsvNotExist = true;

		for(int i = 0; i < infos.Count; i++) {
			Mp3ToSplitPathsInfo info = infos[i];
			ProcessSilDetCSVArgs args = new(info.SilenceDetectCsvPath, info.AuditionMarkersCsvPath);
			await RUN(args);
		}
	}

	public async Task<SDStampsCsvProcessResult> RUN(ProcessSilDetCSVArgs args)
	{
		string csvLog = args.csvLog.NullIfEmptyTrimmed();
		string csvLogPath = args.csvLogPath;

		if(csvLog == null) {
			if(!File.Exists(csvLogPath)) {
				if(NoErrorOnCsvNotExist)
					return null;
				throw new FileNotFoundException("", csvLogPath);
			}
			csvLog = File.ReadAllText(csvLogPath);
		}

		if(csvLog.NotInRange(10, 50_000)) {
			"Invalid content".Print();
			return null;
		}

		SilDetTimeStampsCSV csvLg = new();
		List<TrackTimeStamp> stamps = csvLg.Parse(csvLog);

		int origCnt = stamps.Count;

		HadAddsOrPluses = csvLg.CombineCuts();

		if(!HadAddsOrPluses && IgnoreAllWOutAddsOrPluses)
			return null;

		stamps = csvLg.Stamps;
		int finCnt = stamps.Count;

		if(SaveCsvLog && csvLogPath != null && (HadAddsOrPluses || !SaveCsvLogOnlyOnChange)) {
			csvLog = csvLg.WriteToString();
			await File.WriteAllTextAsync(csvLogPath, csvLog);
		}

		AuditionCsv acsv = new();
		acsv.SetMarkers(stamps, ChapterNamePrefix, firstDesc: $"From mp3-slap silence csv logs - {csvLg.FileName}");

		string audCsv = acsv.WriteCsv(includeHeader: true);

		if(SaveAuditionCsv) {
			await File.WriteAllTextAsync(args.audMarkersCsvPath, audCsv);
		}

		SDStampsCsvProcessResult x = new(stamps, csvLg, acsv, csvLog, audCsv);
		Items.Add(x);
		return x;
	}

}

public record SDStampsCsvProcessResult(
	List<TrackTimeStamp> stamps,
	SilDetTimeStampsCSV csv,
	AuditionCsv acsv,
	string csvLog = null,
	string audMarkersCsv = null);

public record ProcessSilDetCSVArgs(
	string csvLogPath,
	string audMarkersCsvPath,
	string csvLog = null);
