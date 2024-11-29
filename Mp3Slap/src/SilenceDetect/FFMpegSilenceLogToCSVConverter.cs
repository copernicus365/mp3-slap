namespace Mp3Slap.SilenceDetect;

/// <summary>
/// Was named till recently `AlbumLogsWriter` .. ?
/// </summary>
/// <param name="info"></param>
public class FFMpegSilenceLogToCSVConverter
{
	Mp3ToSplitPathsInfo info;

	public FFMpegSilenceLogToCSVConverter(Mp3ToSplitPathsInfo info)
	{
		this.info = info;
	}

	public void Init(double silenceInSecondsMin, bool isFirstRun = true)
	{
		string script = GetFFMpegDetectSilenceScript(silenceInSecondsMin);

		if(isFirstRun && !Directory.Exists(info.LogDirectory))
			Directory.CreateDirectory(info.LogDirectory);
	}

	public void GetAndWriteCsvParsed(double pad, bool noWrite = false)
	{
		string logP = info.SilenceDetectRawLogPath;

		if(!File.Exists(logP)) {
			$"Log file doesn't exist: '{logP}'".Print();
			return;
		}

		string log = File.ReadAllText(logP);
		if(log == null)
			return;

		FFSilenceTracksParser split = new(log);

		List<TrackTimeStamp> tracks = split.Run();

		TrackTimeStampsCsv csv = new() {
			Name = "Daniel",
			SrcPath = info.FilePath,
			LogPath = logP,
			Pad = 0,
			Stamps = tracks
		};

		if(pad > 0)
			tracks.SetPads(pad);

		string result = csv.Write();

		string writePath = info.SilenceDetectCsvParsedLogPath;

		if(!noWrite)
			File.WriteAllText(writePath, result);
	}

	public string GetFFMpegDetectSilenceScript(double silenceInSecondsMin)
	{
		string silStr = silenceInSecondsMin.ToString("0.##");
		return info.FFMpegScriptArgs = $"""-nostats -i '{info.FilePath}' -af silencedetect=noise=-30dB:d={silStr} -f null - 2> '{info.SilenceDetectRawLogPath}' """;
	}

	public static FFSilenceTracksParser ConvertFFMpegSilenceLogToCSV(
		TrackTimeStampsCsv csv,
		double pad,
		//string logP, // info.SilenceDetectRawLogPath;
		//string filePath, // info.FilePath
		//string csvWritePath, // info.SilenceDetectCsvParsedLogPath;
		bool write = true)
	{
		string logP = csv.LogPath;

		if(!File.Exists(logP)) {
			$"Log file doesn't exist: '{logP}'".Print();
			return null;
		}

		string log = File.ReadAllText(logP);
		if(log == null)
			return null;

		FFSilenceTracksParser split = new(log);

		csv.Stamps = split.Run();

		csv.Stamps.SetPads(pad);

		string result = csv.Write();

		if(write)
			File.WriteAllText(csv.CsvLogPath, result);

		return split;
	}

	public static void ConvertFFMpegSilenceLogsToCSVs(string logsDir, double pad, string srcDir = null)
	{
		string[] logsPaths = PathHelper.GetFilesFromDirectory(logsDir, "*.log", includeSubDirectories: false);

		TrackTimeStampsCsv first = null;

		for(int i = 0; i < logsPaths.Length; i++) {
			string path = logsPaths[i];

			TrackTimeStampsCsv tcsv = LogFileNames.GetPathsEtc(path, srcDir);
			if(tcsv == null)
				continue;

			if(first == null) {
				first = tcsv;
				srcDir = first.SrcDir;
			}

			FFSilenceTracksParser ffP = ConvertFFMpegSilenceLogToCSV(tcsv, pad);
		}
	}

}
