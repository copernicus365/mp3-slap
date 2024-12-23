namespace Mp3Slap.General;

public class Mp3ToSplitPathsInfo
{
	public string Directory { get; init; }
	public string LogDirectory { get; init; }
	public string AudioFileName { get; init; }
	public string AudioFilePath { get; init; }
	public string FFSDLogPath { get; set; }
	public string SDTimeStampsCSVPath { get; set; }
	public string AudMarkersCSVPath { get; set; }
	public double SilenceDuration { get; init; }
}
