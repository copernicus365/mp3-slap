namespace Mp3Slap.SilenceDetect;

// silencedetect-write-ffmpeg-script
// ffsilence
public class SilenceDetectWriteFFMpegScriptArgs : SilenceDetectArgsBase
{
	/// <summary>
	/// Scripts to run ffmpeg, make them have relative paths
	/// </summary>
	public bool WriteRelativePaths { get; set; }

	public string AudioFilesSearch { get; set; } = "*.mp3";
}

// convert-ffmpeg-silence-logs-to-csv
// tocsv
public class ConvertFFMpegSilenceLogsToCSVArgs : SilenceDetectArgsBase
{

}

public class SilenceDetectArgsBase
{
	public string Directory { get; set; }

	public string LogFolder { get; set; }

	public string LogFolderFullPath { get; set; }

	public const string DefaultLogFolder = $"logs-{DurationFolderID}s";

	public const string DurationFolderID = "{duration}";

	public double[] SilenceDurations { get; set; }

	public bool? Verbose { get; set; }

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

		// validate durations:

		var durs = SilenceDurations;
		if(durs.IsNulle())
			return new(false, "No durations set");

		if(durs.Any(d => d <= 0))
			return new(false, "Invalid durations");

		if(durs.Distinct().Count() != durs.Length)
			return new(false, "Durations have some duplicates");

		return new(true);
	}

	public string GetLogFolderName(double dur, bool fullPath = false)
		=> (fullPath ? LogFolderFullPath : LogFolder).Replace(DurationFolderID, dur.Round(2).ToString());
}
