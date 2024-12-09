namespace Mp3Slap.SilenceDetect;

public class WriteAuditionMarkerCSVs
{
	public List<StampsCsvGroup> Items = [];

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

	public async Task<StampsCsvGroup> RUN(
		Mp3ToSplitPathsInfo info,
		string silenceDetCsvLog)
	{
		StampsCsvGroup x = await RUN(info.AuditionMarkersCsvPath, silenceDetCsvLog);
		info.Stamps = x.stamps;
		return x;
	}

	public async Task<StampsCsvGroup> RUN(
		string savePath,
		string silenceDetCsvLog)
	{
		TrackTimeStampsCsvV2 csv2 = new();
		List<TrackTimeStamp> stamps = csv2.Parse(silenceDetCsvLog);

		AuditionCsv acsv = new();
		acsv.SetMarkers(stamps, ChapterNamePrefix, firstDesc: $"From mp3-slap silence csv logs - {csv2.FileName}");

		string audCsv = acsv.WriteCsv(includeHeader: true);

		await File.WriteAllTextAsync(savePath, audCsv);

		StampsCsvGroup x = new (stamps, csv2, acsv);
		Items.Add(x);
		return x;
	}

}

public record StampsCsvGroup(List<TrackTimeStamp> stamps, TrackTimeStampsCsvV2 csv, AuditionCsv acsv);

public record SrcDestPaths(string SrcPath, string DestPath);
