namespace Test;

public abstract class FFSilenceDetectBase : BaseTest
{
	public static string GetSampleFFSilLogPATH(string fname)
		=> $"ffsil-logs/{fname}";
		//=> $"sample-ffmpeg-silence-logs/{fname}";

	public string GetSampleFFSilLog(string fname)
		=> DataString(GetSampleFFSilLogPATH(fname));
}
