namespace Mp3Slap;

public class Mp3ToSplitPathsInfo
{
	public string Directory { get; init; }
	public string LogDirectory { get; init; }
	public string FilePath { get; init; }
	public string FileName { get; init; }
	public string SilenceDetectRawLogPath { get; init; }
	public string SilenceDetectCsvParsedLogPath { get; init; }

	public double SilenceDuration { get; init; }

	public List<TrackTimeStamp> Stamps { get; set; }

	public string FFMpegScriptArgs { get; set; }

	public string FFMpegScriptArgsNoLogPath { get; set; }

	public string FFMpegScript => FFMpegScriptArgs == null ? null : "ffmpeg " + FFMpegScriptArgs;

	public string GetFFMpegScript(bool withLogPaths) => "ffmpeg " + (withLogPaths ? FFMpegScriptArgs : FFMpegScriptArgsNoLogPath);

	public void Init()
	{
		FFMpegScriptArgs = SetFFMpegDetectSilenceScript(true);
		FFMpegScriptArgsNoLogPath = SetFFMpegDetectSilenceScript(false);
	}

	public string SetFFMpegDetectSilenceScript(bool writeToLog)
	{
		string silStr = SilenceDuration.ToString("0.##");
		string val = $"""-nostats -i "{FilePath}" -af silencedetect=noise=-30dB:d={silStr} -f null -""";
		if(writeToLog)
			val += $""" 2> "{SilenceDetectRawLogPath}" """;
		return val;
	}

	public string FFSplitOutput { get; set; }
}
