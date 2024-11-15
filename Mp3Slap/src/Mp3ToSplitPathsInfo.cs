namespace Mp3Slap;

public class Mp3ToSplitPathsInfo
{
	public string Directory { get; init; }
	public string LogDirectory { get; init; }
	public string FilePath { get; init; }
	public string FileName { get; init; }
	public string SilenceDetectRawLogPath { get; init; }
	public string SilenceDetectCsvParsedLogPath { get; init; }

	public string FFMpegScriptArgs { get; set; }

	public string FFMpegScript => FFMpegScriptArgs == null ? null : "ffmpeg " + FFMpegScriptArgs;
}
