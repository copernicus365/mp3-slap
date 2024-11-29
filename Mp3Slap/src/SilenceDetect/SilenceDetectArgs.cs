namespace Mp3Slap.SilenceDetect;

// silencedetect-write-ffmpeg-script, ffsilence
public class SilenceDetectWriteFFMpegScriptArgs : SilenceDetectArgs
{
	/// <summary>
	/// Scripts to run ffmpeg, make them have relative paths
	/// </summary>
	public bool WriteRelativePaths { get; set; }

	public string AudioFilesSearchPattern { get; set; } = AudioFilesSearchPatternDef;

	public bool IncludeSubDirectories { get; set; } = false;

	public const string AudioFilesSearchPatternDef = "*.mp3";
}

// convert-ffmpeg-silence-logs-to-csv, tocsv
public class ConvertFFMpegSilenceLogsToCSVArgs : SilenceDetectArgs
{
	public double Pad { get; set; }
}

public class SilenceDetectArgs
{
	public string Directory { get; set; }

	public string LogFolder { get; set; }

	public string LogFolderFullPath { get; set; }

	public const string DefaultLogFolder = $"logs-{DurationFolderID}";

	public const string DurationFolderID = "{duration}";

	public double[] SilenceDurations { get => silenceDurations; set => silenceDurations = value; }
	double[] silenceDurations;

	public bool Verbose { get; set; }

	public SResult INIT()
	{
		Directory = Directory.NullIfEmptyTrimmed();
		if(Directory == null)
			return new(false, "Empty directory");

		if(!Path.IsPathRooted(Directory))
			return new(false, "Directory not rooted");

		Directory = PathHelper.CleanDirPath(Path.GetFullPath(Directory));

		LogFolder = LogFolder.NullIfEmptyTrimmed() ?? DefaultLogFolder;

		LogFolder = PathHelper.CleanDirPath(LogFolder);

		LogFolderFullPath = Path.IsPathRooted(LogFolder)
			? LogFolder
			: PathHelper.CleanDirPath(Path.GetFullPath(Path.Combine(Directory, LogFolder)));

		var r = validateDurs(ref silenceDurations);
		if(!r.Success)
			return r;

		return new(true);
	}

	SResult validateDurs(ref double[] durs)
	{
		if(durs.IsNulle())
			return new(false, "No duration(s) set");

		if(durs.Any(d => d <= 0))
			return new(false, "Invalid durations");

		if(durs.Distinct().Count() != durs.Length)
			return new(false, "Durations have some duplicates");

		return new(true);
	}

	public string GetLogFolderName(double dur, bool fullPath = false)
		=> (fullPath ? LogFolderFullPath : LogFolder).Replace(DurationFolderID, dur.Round(2).ToString());
}
