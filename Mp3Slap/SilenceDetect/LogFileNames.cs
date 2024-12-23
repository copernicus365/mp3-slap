namespace Mp3Slap.SilenceDetect;

/// <summary>
/// Class dedicated establishing the app's convention based naming of
/// logs and directories and so forth.
/// </summary>
public static partial class LogFileNames
{
	public const string log_sildet_ffmpeg_ext = "silencedetect.log";
	public const string log_sildet_csv_ext = "silencedetect-parsed.csv";
	public const string log_sildet_audcsv_ext = "audition-markers.csv";

	public static void SetLogPaths(this Mp3ToSplitPathsInfo info)
	{
		string logDir = info.LogDirectory;
		string fname = info.AudioFileName;
		info.FFSDLogPath = GetLogPath(logDir, fname, parsedTxt: false);
		info.SDTimeStampsCSVPath = GetLogPath(logDir, fname, parsedTxt: true);
		info.AudMarkersCSVPath = GetAuditionMarkersPath(logDir, fname);
	}

	public static string GetLogPath(string logDir, string fileName, bool parsedTxt)
	{
		string logPath = parsedTxt
			? $"{logDir}log#{fileName}#{log_sildet_csv_ext}"
			: $"{logDir}log#{fileName}#{log_sildet_ffmpeg_ext}";
		return logPath;
	}

	public static string GetAuditionMarkersPath(string logDir, string fileName)
	{
		string logPath = $"{logDir}log#{fileName}#{log_sildet_audcsv_ext}";
		return logPath;
	}

	public static string GetAuditionMarkersCsvPathFromSilenceCsvPath(string silenceDetectCsvPath)
	{
		string p = silenceDetectCsvPath;
		if(p.EndsWith(log_sildet_csv_ext, StringComparison.OrdinalIgnoreCase)) {
			return p.CutEnd(log_sildet_csv_ext.Length) + log_sildet_audcsv_ext;
		}
		return p + "-" + log_sildet_audcsv_ext;
	}

	public static string GetSilenceDetCsvPathFromFFMpegLogPath(string fflog, bool forAudMarkers = false)
	{
		string p = fflog;
		string newExt = !forAudMarkers
			? log_sildet_csv_ext
			: log_sildet_audcsv_ext;
		if(p.EndsWith(log_sildet_ffmpeg_ext, StringComparison.OrdinalIgnoreCase)) {
			return p.CutEnd(log_sildet_ffmpeg_ext.Length) + newExt;
		}
		return p + "-" + newExt;
	}

	public static string GetAuditionMarkersPathFromCsvLog(string csvLogPath)
	{
		ArgumentException.ThrowIfNullOrEmpty(csvLogPath);

		if(!csvLogPath.EndsWith(log_sildet_csv_ext))
			return null;

		string npath = csvLogPath.CutEnd(log_sildet_csv_ext.Length);
		return npath + log_sildet_audcsv_ext;
	}

	public static string GetParsedCsvPathFromRaw(string file)
	{
		if(!file.EndsWith(log_sildet_ffmpeg_ext))
			return null;
		string res = file.CutEnd(log_sildet_ffmpeg_ext.Length) + log_sildet_csv_ext;
		return res;
	}

	public static string GetFileNameMinusLogNaming(string file)
	{
		Regex rx = Rx1();
		var m = rx.Match(file);

		if(!m.Success || m.Groups.Count != 2)
			return null;

		string val = m.Groups[1].Value;
		return val;
	}

	[GeneratedRegex("""#(.*)#""")]
	private static partial Regex Rx1();
}
