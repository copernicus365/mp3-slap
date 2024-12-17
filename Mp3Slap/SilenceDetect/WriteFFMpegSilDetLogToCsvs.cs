namespace Mp3Slap.SilenceDetect;

public class WriteFFMpegSilDetLogToCsvs
{
	FFSilenceDetToTimeStampsParser ffToTracksParser;
	SilDetTimeStampsCSV csvWriter;
	ProcessSilDetCSV procSDCsv;

	public double Pad { get; init; }

	public bool WriteAuditionMarkerCsvs { get; init; }


	public async Task<SResult> RUN(Mp3ToSplitPathsInfo info)
	{
		var res = await RUN(
			ffLogPath: info.SilenceDetectRawLogPath,
			csvLogPath: info.SilenceDetectCsvPath,
			audMarkersCsvPath: info.AuditionMarkersCsvPath,
			ffLogContent: info.FFSplitOutput,
			audioFilePath: info.FilePath);

		info.Stamps = ffToTracksParser.Stamps;
		return res;
	}

	public async Task<SResult> RUN(
		string ffLogPath,
		string csvLogPath,
		string audMarkersCsvPath = null,
		string ffLogContent = null,
		string audioFilePath = null,
		bool allowOverwriteCsv = true)
	{
		if(!allowOverwriteCsv && File.Exists(csvLogPath))
			return new SResult(false, "Save path exists and overwrite not allowed");

		if(ffLogContent.IsNulle()) {
			ffLogContent = File.ReadAllText(ffLogPath);

			if(ffLogContent.NotInRange(400, 100_000))
				return new SResult(false, "Source doesn't exist or invalid length");
		}

		ffToTracksParser = new(text: ffLogContent, pad: Pad);

		ffToTracksParser.Parse(setMeta: true);

		FFAudioMeta meta = ffToTracksParser.Meta;
		//split.Stamps.SetPads(args.Pad);

		csvWriter = new(); //SDTimeStampsCsv
		csvWriter.InitForWrite(Pad, ffToTracksParser.Stamps, meta: meta, filePath: audioFilePath);

		string csvContent = csvWriter.WriteToString();

		File.WriteAllText(csvLogPath, csvContent);

		if(WriteAuditionMarkerCsvs) {
			procSDCsv = new() {
				ResaveCsvLogOnChange = false, // we JUST saved it couple lines above
				SaveAuditionCsv = true
			};
			ProcessSilDetCSVArgs args = new (csvLogPath, audMarkersCsvPath, csvContent);
			await procSDCsv.RUN(args);
		}
		return new SResult(true);
	}
}
