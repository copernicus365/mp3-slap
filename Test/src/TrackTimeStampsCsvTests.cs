namespace Test;

public class TrackTimeStampsCsvTests : BaseTest
{
	public bool WriteParsedLogs = false;

	[Fact]
	public void TestParseCuts1()
	{
		//string csvText = GetDataDirPath($"{SampleResultsDirName}/results-dan3.csv");
		string csvText = DataString($"{SampleResultsDirName}/log#gen1.mp3#silencedetect-parsed--n0.csv"); // niv-suchet-27-daniel.mp3#silencedetect-parsed--n1.csv");

		TrackTimeStampsCsv csv = new();
		csv.Parse(csvText);

		int expCount = 10;

		True(csv.Count == expCount);
		csv.CombineCuts();

		True(csv.Valid);
		True(csv.Count == expCount);

		string result = csv.Write();

		string writePath = GetDataDirPath($"{SampleResultsDirName}/write-temp/parsed-combine-1.csv");

		if(WriteParsedLogs)
			File.WriteAllText(writePath, result);
	}

	[Fact]
	public void ParseTimeStampCSV_ShorterTimeSpanStamps()
	{
		// NOTE:
		// "1:35.47"  instead of "0:01:35.47"
		// "03:30.65" instead of "0:03:30.65"

		// 
		string value = """ "0:02:58.09", "0:04:29.47", "0:01:31.38", "2.34", "0.30", "0:02:57.79", "0:04:29.77", "0:01:31.98", """;

		var ts = TrackTimeStamp.ParseCsvString(value);

		True(ts != null);
		True(ts.Start == TimeSpan.Parse("0:02:58.09"));
		True(ts.End == TimeSpan.Parse("0:04:29.47"));
		True(ts.Duration == TimeSpan.Parse("0:01:31.38"));
		True(ts.SilenceDuration.TotalSeconds == double.Parse("2.34"));
		True(ts.PaddedStart == TimeSpan.Parse("0:02:57.79"));
		True(ts.PaddedEnd == TimeSpan.Parse("0:04:29.77"));
		True(ts.PaddedDuration == TimeSpan.Parse("0:01:35.47"));
		True(ts.Pad.TotalSeconds == double.Parse("2.42"));
	}

	public void ParseTimeStampCSV_ShorterTimeSpanStamps_OLD()
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
}
