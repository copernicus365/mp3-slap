namespace Mp3Slap.SilenceDetect;

public class WriteFFMpegSilDetLogToCsvs
{
	FFSDLogToTimeStampsParser Parser;
	ProcessSilDetCSV procSDCsv;
	public SilDetTimeStampsCSV CSV { get; private set; }

	public double Pad { get; init; }

	public bool WriteAuditionMarkerCsvs { get; init; }

	public async Task<SResult> RUN(
		Mp3ToSplitPathsInfo info,
		string ffSDLogContent,
		bool allowOverwriteCsv = true)
	{
		//var res = await RUN(
		//	info,
		//	ffLogPath: info.SilenceDetectRawLogPath,
		//	csvLogPath: info.SilenceDetectCsvPath,
		//	audMarkersCsvPath: info.AuditionMarkersCsvPath,
		//	ffLogContent: ffSplitOutput,
		//	audioFilePath: info.AudioFilePath);

		if(!allowOverwriteCsv && File.Exists(info.SDTimeStampsCSVPath))
			return new SResult(false, "Save path exists and overwrite not allowed");

		if(ffSDLogContent.IsNulle()) {
			ffSDLogContent = File.ReadAllText(info.FFSDLogPath);

			if(ffSDLogContent.NotInRange(400, 100_000))
				return new SResult(false, "Source doesn't exist or invalid length");
		}

		Parser = new(text: ffSDLogContent, pad: Pad);

		Parser.Parse(setMeta: true);

		//split.Stamps.SetPads(args.Pad);

		CSV = new();
		CSV.InitForWrite(
			Parser.Stamps,
			duration: info.SilenceDuration,
			pad: Pad,
			fileName: null,
			filePath: info.AudioFilePath,
			meta: Parser.Meta);

		//csvWriter.InitForWrite(
		//	Pad,
		//	ffToTracksParser.Stamps,
		//	meta: ffToTracksParser.Meta,
		//	filePath: audioFilePath);

		string csvContent = CSV.WriteToString();

		File.WriteAllText(info.SDTimeStampsCSVPath, csvContent);

		if(WriteAuditionMarkerCsvs) {
			procSDCsv = new() {
				SaveCsvLog = false, // we JUST saved it couple lines above
				SaveAuditionCsv = true
			};

			ProcessSilDetCSVArgs args = new(info.SDTimeStampsCSVPath, info.AudMarkersCSVPath, csvContent);
			await procSDCsv.RUN(args);
		}
		return new SResult(true);
	}
}

//public async Task<SResult> RUNZZ(
//	Mp3ToSplitPathsInfo info,
//	string ffLogPath,
//	string csvLogPath,
//	string audMarkersCsvPath = null,
//	string ffLogContent = null,
//	string audioFilePath = null,
//	bool allowOverwriteCsv = true)
//{
//	if(!allowOverwriteCsv && File.Exists(info.SDTimeStampsCSVPath))
//		return new SResult(false, "Save path exists and overwrite not allowed");

//	if(ffLogContent.IsNulle()) {
//		ffLogContent = File.ReadAllText(info.FFSDLogPath);

//		if(ffLogContent.NotInRange(400, 100_000))
//			return new SResult(false, "Source doesn't exist or invalid length");
//	}

//	Parser = new(text: ffLogContent, pad: Pad);

//	Parser.Parse(setMeta: true);

//	//split.Stamps.SetPads(args.Pad);

//	csvWriter = new();
//	csvWriter.InitForWrite(
//		Parser.Stamps,
//		duration: info.SilenceDuration,
//		pad: Pad,
//		fileName: null,
//		filePath: audioFilePath,
//		meta: Parser.Meta);

//	//csvWriter.InitForWrite(
//	//	Pad,
//	//	ffToTracksParser.Stamps,
//	//	meta: ffToTracksParser.Meta,
//	//	filePath: audioFilePath);

//	string csvContent = csvWriter.WriteToString();

//	File.WriteAllText(info.SDTimeStampsCSVPath, csvContent);

//	if(WriteAuditionMarkerCsvs) {
//		procSDCsv = new() {
//			SaveCsvLog = false, // we JUST saved it couple lines above
//			SaveAuditionCsv = true
//		};
//		ProcessSilDetCSVArgs args = new (info.SDTimeStampsCSVPath, audMarkersCsvPath, csvContent);
//		await procSDCsv.RUN(args);
//	}
//	return new SResult(true);
//}
