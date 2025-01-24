using Mp3Slap.General;

namespace Test;

public abstract class SilenceDetectBase : BaseTest
{
	public static string GetSampleFFSilLogPATH(string fname)
		=> $"ffsil-logs/{fname}";
		//=> $"sample-ffmpeg-silence-logs/{fname}";

	public string GetSampleFFSilLog(string fname)
		=> DataString(GetSampleFFSilLogPATH(fname));

	public static List<TrackTimeStamp> GetStamps1()
	{
		List<TrackTimeStamp> stamps = [
			new("0:00:00.00", "0:05:44.01", "3.79"),
			new("0:05:47.80", "0:09:36.13", "3.76"),
			new("0:09:39.89", "0:13:49.81", "4.07"),
			new("0:13:53.89", "0:18:08.20", "3.97"),
			new("0:18:12.17", "0:22:02.00", "4.35"),
		];
		return stamps;
	}


	public string csvLog_gen()
		=> DataString($"{SampleResultsDirName}/log#01-genesis.mp3#silencedetect-parsed.csv");

	public string csvLog_gen_subs()
		=> DataString($"{SampleResultsDirName}/log#01-genesis-3.1-with-subs.mp3#silencedetect-parsed.csv");

	public string csvLog_1num_v1()
		=> DataString($"{SampleResultsDirName}/samples1/log-1time-v1.csv");

	public string csvLog_2num_v1()
		=> DataString($"{SampleResultsDirName}/samples1/log-2time-v1.csv");

	public string csvLog_2num_adds_cuts_simple1()
		=> DataString($"{SampleResultsDirName}/samples1/log-2time-with-adds-cuts-simple1.csv");

	public string csvLog_2num_adds_cuts_simple2()
		=> DataString($"{SampleResultsDirName}/samples1/log-2time-with-adds-cuts-simple2.csv");

	public string csvLog_gen_short()
		=> DataString($"{SampleResultsDirName}/log#01-gen-short.mp3#silencedetect-parsed.csv");
}
