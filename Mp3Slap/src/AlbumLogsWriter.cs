namespace Mp3Slap;

public class AlbumLogsWriter(Mp3ToSplitPathsInfo info)
{
	public void Init(double silenceInSecondsMin, bool isFirstRun = true)
	{
		string script = GetFFMpegDetectSilenceScript(silenceInSecondsMin);

		if(isFirstRun && !Directory.Exists(info.LogDirectory))
			Directory.CreateDirectory(info.LogDirectory);
	}

	public void GetAndWriteCsvParsed(bool noWrite = false)
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

		string result = csv.Write();

		string writePath = info.SilenceDetectCsvParsedLogPath;

		if(!noWrite)
			File.WriteAllText(writePath, result);
	}

	public string GetFFMpegDetectSilenceScript(
		double silenceInSecondsMin)
	{
		string silStr = silenceInSecondsMin.ToString("0.##");
		return info.FFMpegScriptArgs = $"""-nostats -i '{info.FilePath}' -af silencedetect=noise=-30dB:d={silStr} -f null - 2> '{info.SilenceDetectRawLogPath}' """;
	}

}
