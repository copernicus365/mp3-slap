namespace Mp3Slap.SilenceDetect;

public class ProcessSilDetCSV
{
	public bool ResaveCsvLogOnChange { get; set; } = true;

	public bool SaveAuditionCsv { get; set; } = true;

	public List<SDStampsCsvProcessResult> Items = [];

	public string ChapterNamePrefix { get; set; } = "Ch ";

	public async Task RUN(List<Mp3ToSplitPathsInfo> infos)
	{
		if(infos.IsNulle()) return;

		for(int i = 0; i < infos.Count; i++) {
			Mp3ToSplitPathsInfo info = infos[i];

			if(!File.Exists(info.SilenceDetectCsvPath))
				continue;

			string silenceDetCsvLog = File.ReadAllText(info.SilenceDetectCsvPath);
			if(silenceDetCsvLog.IsNulle())
				continue;

			await RUN(info, silenceDetCsvLog);
		}
	}

	public async Task<SDStampsCsvProcessResult> RUN(
		Mp3ToSplitPathsInfo info,
		string silenceDetCsvLog)
	{
		SDStampsCsvProcessResult x = await RUN(info.AuditionMarkersCsvPath, silenceDetCsvLog, info.SilenceDetectCsvPath);
		info.Stamps = x.stamps;
		return x;
	}

	public async Task<SDStampsCsvProcessResult> RUN(
		string savePath,
		string silenceDetCsvLog,
		string silenceDetCsvLogPath = null)
	{
		SDTimeStampsCsv csvLg = new();
		List<TrackTimeStamp> stamps = csvLg.Parse(silenceDetCsvLog);

		int origCnt = stamps.Count;

		csvLg.CombineCuts();

		stamps = csvLg.Stamps;
		int finCnt = stamps.Count;

		if(ResaveCsvLogOnChange && silenceDetCsvLogPath != null) {
			silenceDetCsvLog = csvLg.WriteToString();
			await File.WriteAllTextAsync(silenceDetCsvLogPath, silenceDetCsvLog);
		}

		AuditionCsv acsv = new();
		acsv.SetMarkers(stamps, ChapterNamePrefix, firstDesc: $"From mp3-slap silence csv logs - {csvLg.FileName}");

		string audCsv = acsv.WriteCsv(includeHeader: true);

		if(SaveAuditionCsv) {
			await File.WriteAllTextAsync(savePath, audCsv);
		}

		SDStampsCsvProcessResult x = new(stamps, csvLg, acsv, silenceDetCsvLog, audCsv);
		Items.Add(x);
		return x;
	}

}

public record SDStampsCsvProcessResult(
	List<TrackTimeStamp> stamps,
	SDTimeStampsCsv csv,
	AuditionCsv acsv,
	string csvLongString = null,
	string audMarkersCsvString = null);

public record SrcDestPaths(string SrcPath, string DestPath);
