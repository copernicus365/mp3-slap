namespace Mp3Slap.General;

public class SilDetScripts
{
	public string FFMpegScriptArgs { get; set; }

	public string FFMpegScriptArgsNoLogPath { get; set; }

	public string FFMpegScript => FFMpegScriptArgs == null ? null : "ffmpeg " + FFMpegScriptArgs;

	public string GetFFMpegScript(bool withLogPaths) => "ffmpeg " + (withLogPaths ? FFMpegScriptArgs : FFMpegScriptArgsNoLogPath);

	public SilDetScripts Init(Mp3ToSplitPathsInfo info, double SilenceDuration)
		=> Init(info.AudioFilePath, info.SilenceDetectRawLogPath, SilenceDuration);

	public SilDetScripts Init(string audioFilePath, string silenceDetectRawLogPath, double silenceDuration)
	{
		FFMpegScriptArgs = SetFFMpegDetectSilenceScript(true);
		FFMpegScriptArgsNoLogPath = SetFFMpegDetectSilenceScript(false);
		return this;
		string SetFFMpegDetectSilenceScript(bool writeToLog)
		{
			string silStr = silenceDuration.ToString("0.##");
			string val = $"""-nostats -i "{audioFilePath}" -af silencedetect=noise=-30dB:d={silStr} -f null -""";
			if(writeToLog)
				val += $""" 2> "{silenceDetectRawLogPath}" """;
			return val;
		}
	}
}
