namespace Test;

public abstract class FFSilenceDetectBase : BaseTest
{
	public static string _getSampleFFMpegSilenceLogPATH(string fname)
		=> $"sample-ffmpeg-silence-logs/{fname}";

	public string _GetFFSilencesRawLog(string fname)
		=> DataString(_getSampleFFMpegSilenceLogPATH(fname));
}
