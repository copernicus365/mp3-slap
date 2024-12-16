namespace Mp3Slap.SilenceDetect;

public class WriteFFMpegSilDetLogToCsvs
{
	public double Pad { get; init; }

	public bool WriteAuditionMarkerCsvs { get; init; }


	FFSilenceTracksParser split;
	TrackTimeStampsCsv csv2;
	WriteAuditionMarkerCSVs wcsv;


	public async Task<SResult> RUN(Mp3ToSplitPathsInfo info)
	{
		var res = await RUN(
			ffLogPath: info.SilenceDetectRawLogPath,
			csvPath: info.SilenceDetectCsvPath,
			audMarkersPath: info.AuditionMarkersCsvPath,
			ffLogContent: info.FFSplitOutput,
			audioFilePath: info.FilePath);

		info.Stamps = split.Stamps;
		return res;
	}

	public async Task<SResult> RUN(
		string ffLogPath,
		string csvPath,
		string audMarkersPath = null,
		string ffLogContent = null,
		string audioFilePath = null,
		bool allowOverwriteCsv = true)
	{
		if(!allowOverwriteCsv && File.Exists(csvPath))
			return new SResult(false, "Save path exists and overwrite not allowed");

		if(ffLogContent.IsNulle()) {
			ffLogContent = File.ReadAllText(ffLogPath);

			if(ffLogContent.NotInRange(400, 100_000))
				return new SResult(false, "Source doesn't exist or invalid length");
		}

		split = new(
			text: ffLogContent,
			pad: Pad);

		split.Parse(setMeta: true);

		FFAudioMeta meta = split.Meta;
		//split.Stamps.SetPads(args.Pad);

		csv2 = new();
		csv2.InitForWrite(null, Pad, split.Stamps, filePath: audioFilePath);

		string csvContent = csv2.Write();

		File.WriteAllText(csvPath, csvContent);

		if(WriteAuditionMarkerCsvs) {
			wcsv = new();
			await wcsv.RUN(audMarkersPath, csvContent);
		}

		return new SResult(true);
	}
}
