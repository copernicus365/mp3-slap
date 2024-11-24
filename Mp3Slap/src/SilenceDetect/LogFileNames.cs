namespace Mp3Slap.SilenceDetect;

public partial class LogFileNames
{
	public static string GetLogPath(string logDir, string fileName, bool parsedTxt)
	{
		string logPath = parsedTxt
			? $"{logDir}log#{fileName}#silencedetect-parsed.csv"
			: $"{logDir}log#{fileName}#silencedetect.log";
		return logPath;
	}


	public static string GetParsedCsvPathFromRaw(string file)
	{
		if(!file.EndsWith("silencedetect.log"))
			return null;
		string res = file.CutEnd("silencedetect.log".Length) + "silencedetect-parsed.csv";
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


	public static TrackTimeStampsCsv GetPathsEtc(string rawLogPath, double pad, string srcDir = null)
	{
		rawLogPath = PathHelper.CleanPath(rawLogPath);

		string rFileName = Path.GetFileName(rawLogPath);

		string dir = PathHelper.CleanDirPath(Path.GetDirectoryName(rawLogPath));

		if(srcDir != null)
			srcDir = PathHelper.CleanDirPath(srcDir);
		else
			srcDir = PathHelper.CleanDirPath(Path.GetDirectoryName(dir.CutEnd(1)));

		string fileName = GetFileNameMinusLogNaming(rFileName);

		TrackTimeStampsCsv csv = new() {
			Name = fileName,
			FileName = fileName,
			SrcDir = srcDir,
			SrcPath = srcDir + fileName,
			LogPath = rawLogPath,
			CsvLogPath = GetLogPath(dir, fileName, parsedTxt: true),
			Pad = pad, // > 0 ? pad : 3,
		};
		return csv;
	}

	[GeneratedRegex("""#(.*)#""")]
	private static partial Regex Rx1();

}
