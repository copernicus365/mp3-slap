namespace Mp3Slap.SilenceDetect;

public class WriteFFMpegSilDetLogToCsvs
{
	public double Pad { get; init; }

	public bool WriteAuditionMarkerCsvs { get; init; }


	FFSilenceTracksParser split;
	TrackTimeStampsCsvV2 csv2;
	WriteAuditionMarkerCSVs wcsv;


	public async Task RUN(Mp3ToSplitPathsInfo info)
	{
		await RUN(
			ffLogPath: info.SilenceDetectRawLogPath,
			csvPath: info.SilenceDetectCsvPath,
			audMarkersPath: info.AuditionMarkersCsvPath,
			ffLogContent: info.FFSplitOutput,
			audioFilePath: info.FilePath);

		info.Stamps = split.Stamps;
	}

	public async Task RUN(
		string ffLogPath,
		string csvPath,
		string audMarkersPath = null,
		string ffLogContent = null,
		string audioFilePath = null)
	{
		ffLogContent ??= File.ReadAllText(ffLogPath);

		split = new(
			text: ffLogContent,
			pad: Pad);

		var stamps = split.Run();
		//split.Stamps.SetPads(args.Pad);

		csv2 = new();
		csv2.InitForWrite(null, Pad, split.Stamps, filePath: audioFilePath);

		string csvContent = csv2.Write();

		File.WriteAllText(csvPath, csvContent);

		if(WriteAuditionMarkerCsvs) {
			wcsv = new();
			await wcsv.RUN(audMarkersPath, csvContent);
		}
	}
}
