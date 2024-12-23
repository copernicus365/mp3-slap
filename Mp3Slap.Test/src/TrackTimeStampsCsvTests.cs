using Mp3Slap.General;

namespace Test;

public class TrackTimeStampsCsvTests : BaseTest
{
	public bool WriteParsedLogs = false;

	[Fact]
	public void ParseLargeCsv_WithSubs_Gen()
	{
		string csvText = csvLog_gen_subs();

		SilDetTimeStampsCSV csv = new();
		csv.Parse(csvText);

		int expCount = 54;

		True(csv.Count == expCount);
		True(csv.CombineCuts());

		True(csv.Stamps.Count > 0);

		True(csv.Count == 50);

		string result = csv.WriteToString();

		string writePath = GetDataDirPath($"{SampleResultsDirName}/write-temp/parsed-combine-1.csv");

		if(WriteParsedLogs)
			File.WriteAllText(writePath, result);

	}

	[Fact]
	public void ParseLargeCsvDoc1_Gen()
	{
		string csvText = csvLog_gen();

		SilDetTimeStampsCSV csv = new();
		csv.Parse(csvText);

		int expCount = 51;

		True(csv.Count == expCount);

		True(csv.CombineCuts());

		True(csv.Stamps.Count > 0);

		True(csv.Count == 50);

		string result = csv.WriteToString();

		string writePath = GetDataDirPath($"{SampleResultsDirName}/write-temp/parsed-combine-1.csv");

		if(WriteParsedLogs)
			File.WriteAllText(writePath, result);

	}

	[Fact]
	public void ParseStamp1()
	{
		string value = """ "0:09:39.89", "0:13:49.81", "0:04:09.92", "4.07", "0.30", "0:09:39.59", "0:13:53.59", "0:04:13.99" """;

		var ts = TrackTimeStamp.ParseCsvString(value);

		True(ts != null);
		True(ts.Start == TimeSpan.Parse("0:09:39.89"));
		True(ts.End == TimeSpan.Parse("0:13:49.81"));
		True(ts.Duration == TimeSpan.Parse("0:04:09.92"));
		True(ts.SilenceDuration.TotalSeconds == double.Parse("4.07"));
		True(ts.PaddedStart == TimeSpan.Parse("0:09:39.59"));
		True(ts.PaddedEnd == TimeSpan.Parse("0:13:53.58")); // changed from .59 -> .58, just ignore
		True(ts.PaddedDuration == TimeSpan.Parse("0:04:13.99"));
		True(ts.Pad.TotalSeconds == double.Parse("0.30"));
	}

	void ParseTimeStampCSV_ShorterTimeSpanStamps_OLD()
	{
		// NOTE:
		// "1:35.47"  instead of "0:01:35.47"
		// "03:30.65" instead of "0:03:30.65"

		//"0:02:58.09", "0:04:29.47", "0:01:31.38", "2.34", "0.30", "0:02:57.79", "0:04:29.77", "0:01:31.98", 
		string value = """ "03:30.65", "0:05:06.12", "1:35.47", "2.42" """;

		var ts = TrackTimeStamp.ParseCsvString(value);

		True(ts != null);
		True(ts.Start == TimeSpan.Parse("0:03:30.65"));
		True(ts.End == TimeSpan.Parse("0:05:06.12"));
		True(ts.Duration == TimeSpan.Parse("0:01:35.47"));
		True(ts.SilenceDuration.TotalSeconds == double.Parse("2.42"));
	}

	string csvLog_gen()
		=> DataString($"{SampleResultsDirName}/log#01-genesis.mp3#silencedetect-parsed.csv");

	string csvLog_gen_subs()
		=> DataString($"{SampleResultsDirName}/log#01-genesis-3.1-with-subs.mp3#silencedetect-parsed.csv");

}
