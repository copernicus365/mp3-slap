//namespace Mp3Slap.SilenceDetect;

///// <summary>
///// Was named till recently `AlbumLogsWriter` .. ?
///// </summary>
///// <param name="info"></param>
//public class FFMpegSilenceLogToCSVConverter //(Mp3ToSplitPathsInfo info)
//{
//	//public string GetFFMpegDetectSilenceScript(double silenceInSecondsMin)
//	//{
//	//	string silStr = silenceInSecondsMin.ToString("0.##");
//	//	return info.FFMpegScriptArgs = $"""-nostats -i '{info.FilePath}' -af silencedetect=noise=-30dB:d={silStr} -f null - 2> '{info.SilenceDetectRawLogPath}' """;
//	//}

//	public static FFSilenceTracksParser ConvertFFMpegSilenceLogToCSV(
//		TrackTimeStampsCsv csv,
//		double pad,
//		bool write = true)
//	{
//		string logP = csv.LogPath;

//		if(!File.Exists(logP)) {
//			$"Log file doesn't exist: '{logP}'".Print();
//			return null;
//		}

//		string log = File.ReadAllText(logP);
//		if(log == null)
//			return null;

//		return ConvertFFMpegSilenceLogToCSV(log, csv, pad, write);
//	}

//	public static FFSilenceTracksParser ConvertFFMpegSilenceLogToCSV(
//		string log,
//		TrackTimeStampsCsv csv,
//		double pad,
//		bool write = true)
//	{
//		if(log == null)
//			return null;

//		FFSilenceTracksParser split = new(log, pad);

//		csv.Stamps = split.Parse();

//		csv.Stamps.SetPads(pad);

//		string result = csv.Write();

//		if(write)
//			File.WriteAllText(csv.CsvLogPath, result);

//		return split;
//	}

//	//public static List<FFSilenceTracksParser> ConvertFFMpegSilenceLogsToCSVs(string logsDir, double pad, string srcDir = null)
//	//{
//	//	string[] logsPaths = PathHelper.GetSilenceDetectLogPaths(logsDir);

//	//	TrackTimeStampsCsv first = null;

//	//	List<FFSilenceTracksParser> parsed = [];

//	//	for(int i = 0; i < logsPaths.Length; i++) {
//	//		string path = logsPaths[i];

//	//		TrackTimeStampsCsv tcsv = LogFileNames.GetSilenceLogPathsEtc(path, srcDir);
//	//		if(tcsv == null)
//	//			continue;

//	//		if(first == null) {
//	//			first = tcsv;
//	//			srcDir = first.SrcDir;
//	//		}

//	//		FFSilenceTracksParser ffP = ConvertFFMpegSilenceLogToCSV(tcsv, pad);

//	//		parsed.Add(ffP);
//	//	}
//	//	return parsed;
//	//}

//	//public static FFSilenceTracksParser GetParser(string path, string srcDir, double pad)
//	//{
//	//	TrackTimeStampsCsv tcsv = LogFileNames.GetSilenceLogPathsEtc(path, srcDir);
//	//	if(tcsv == null)
//	//		return null;

//	//	//if(first == null) {
//	//	//	first = tcsv;
//	//	//	srcDir = first.SrcDir;
//	//	//}

//	//	FFSilenceTracksParser ffp = ConvertFFMpegSilenceLogToCSV(tcsv, pad);
//	//	return ffp;
//	//}
//}
