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
			ProcessSilDetCSVArgs args = new(info.SDTimeStampsCSVPath, info.AudMarkersCSVPath);
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

		SilDetTimeStampsCSVParser cparser = new();
		SilDetTimeStampsCSVWriter writer = new();

		cparser.Parse(csvLog);
		List<TrackTimeStamp> stamps = cparser.Stamps;

		int origCnt = stamps.Count;

		HadAddsOrPluses = cparser.CombineCuts();

		if(!HadAddsOrPluses && IgnoreAllWOutAddsOrPluses)
			return null;

		stamps = cparser.Stamps;
		int finCnt = stamps.Count;

		if(SaveCsvLog && csvLogPath != null && (HadAddsOrPluses || !SaveCsvLogOnlyOnChange)) {
			csvLog = writer.WriteToString();
			await File.WriteAllTextAsync(csvLogPath, csvLog);
		}

		AuditionCsv acsv = new();
		acsv.SetMarkers(stamps, ChapterNamePrefix, firstDesc: $"From mp3-slap silence csv logs - {cparser.Meta.fileName}");

		string audCsv = acsv.WriteCsv(includeHeader: true);

		if(SaveAuditionCsv) {
			await File.WriteAllTextAsync(args.audMarkersCsvPath, audCsv);
		}

		SDStampsCsvProcessResult x = new(stamps, writer, acsv, csvLog, audCsv);
		Items.Add(x);
		return x;
	}

}

public record SDStampsCsvProcessResult(
	List<TrackTimeStamp> stamps,
	SilDetTimeStampsCSVWriter csv,
	AuditionCsv acsv,
	string csvLog = null,
	string audMarkersCsv = null);

public record ProcessSilDetCSVArgs(
	string csvLogPath,
	string audMarkersCsvPath,
	string csvLog = null);
