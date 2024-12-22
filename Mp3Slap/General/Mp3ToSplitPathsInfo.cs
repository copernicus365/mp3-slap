namespace Mp3Slap.General;

public class Mp3ToSplitPathsInfo
{
	public string Directory { get; init; }
	public string LogDirectory { get; init; }
	public string AudioFilePath { get; init; }
	public string AudioFileName { get; init; }
	public string SilenceDetectRawLogPath { get; set; }
	public string SilenceDetectCsvPath { get; set; }
	public string AuditionMarkersCsvPath { get; set; }
	public double SilenceDuration { get; init; }
}
