namespace Mp3Slap.General;

public class SilDetScripts
{
	public string FFMpegScriptArgs { get; set; }

	public string FFMpegScriptArgsNoLogPath { get; set; }

	public string FFMpegScript => FFMpegScriptArgs == null ? null : "ffmpeg " + FFMpegScriptArgs;

	public string GetFFMpegScript(bool withLogPaths) => "ffmpeg " + (withLogPaths ? FFMpegScriptArgs : FFMpegScriptArgsNoLogPath);

	public SilDetScripts Init(Mp3ToSplitPathsInfo info, double SilenceDuration)
	{
		FFMpegScriptArgs = setFFMpegDetectSilenceScript(true);
		FFMpegScriptArgsNoLogPath = setFFMpegDetectSilenceScript(false);
		return this;

		string setFFMpegDetectSilenceScript(bool writeToLog)
		{
			string silStr = SilenceDuration.ToString("0.##");
			string val = $"""-nostats -i "{info.AudioFilePath}" -af silencedetect=noise=-30dB:d={silStr} -f null -""";
			if(writeToLog)
				val += $""" 2> "{info.FFSDLogPath}" """;
			return val;
		}
	}
}
