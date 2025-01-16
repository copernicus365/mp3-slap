using Mp3Slap.General;
using Mp3Slap.SilenceDetect;

using Test;

namespace Mp3Slap.Test;

public class WriterTests1 : BaseTest
{
	static TimeSpan pd = TimeSpan.FromSeconds(0.3);

	[Fact]
	public void ParseLargeCsv_WithSubs_Gen()
	{
		SilDetTimeStampsMeta meta = new(pad: 0.3, duration: "04:04:09.81");
		SilDetTimeStampsCSVWriter wr = new();

		wr.InitForWrite(stamps: getStamps1(), meta);

		string csv = wr.WriteToString(includeCSVHeader: true);

		// {"duration":3.3,"pad":0.3,"meta":{"duration":"04:04:09.8100000","start":0.025056}}

		string writePath = GetDataDirPath($"{SampleResultsDirName}/write-temp/parsed-combine-2.csv");

		bool WriteParsedLogs = false;
		if(WriteParsedLogs)
			File.WriteAllText(writePath, csv);
	}


	List<TrackTimeStamp> getStamps1()
	{
		List<TrackTimeStamp> stamps = [
			tss("0:00:00.00", "0:05:44.01", "3.79"),
			tss("0:05:47.80", "0:09:36.13", "3.76"),
			tss("0:09:39.89", "0:13:49.81", "4.07"),
			tss("0:13:53.89", "0:18:08.20", "3.97"),
			tss("0:18:12.17", "0:22:02.00", "4.35"),
		];
		return stamps;
		/*
0:00:00.00, 0:05:44.01, 0:05:44.01, 3.79, 0.00, 0:00:00.00, 0:05:47.80, 0:05:47.80, 
0:05:47.80, 0:09:36.13, 0:03:48.33, 3.76, 0.30, 0:05:47.50, 0:09:39.59, 0:03:52.08, 
0:09:39.89, 0:13:49.81, 0:04:09.92, 4.07, 0.30, 0:09:39.59, 0:13:53.59, 0:04:13.99, 
0:13:53.89, 0:18:08.20, 0:04:14.31, 3.97, 0.30, 0:13:53.59, 0:18:11.87, 0:04:18.28, 
0:18:12.17, 0:22:02.00, 0:03:49.82, 4.35, 0.30, 0:18:11.87, 0:22:06.05, 0:03:54.17, 
		 */
	}

	static TrackTimeStamp tss(params string[] arr)
	{
		TrackTimeStamp v = new(ts(arr[0]), ts(arr[1]), TimeSpan.FromSeconds(arr[2].ToDouble()), pd);
		return v;
	}

	static TimeSpan ts(string v) => TimeSpan.Parse(v);
}
