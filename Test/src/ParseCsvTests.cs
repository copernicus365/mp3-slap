namespace Test;

public class ParseCsvTests : BaseTest
{
	[Fact]
	public void TestParseCuts1()
	{
		//string csvText = GetDataDirPath($"{SampleResultsDirName}/results-dan3.csv");
		string csvText = DataString($"{SampleResultsDirName}/results-dan1.csv");

		TrackTimeStampsCsv csv = new();
		csv.Parse(csvText);

		True(csv.Count == 17);
		csv.CombineCuts();

		True(csv.Valid);
		True(csv.Count == 10);

		string result = csv.Write();

		string writePath = GetDataDirPath($"{SampleResultsDirName}/results-dan4-test-combine-cuts.csv");
		File.WriteAllText(writePath, result);
	}


	[Fact]
	public void ParseTimeStampCSV_ShorterTimeSpanStamps()
	{
		// NOTE:
		// "1:35.47"  instead of "0:01:35.47"
		// "03:30.65" instead of "0:03:30.65"

		string value = """ "03:30.65", "0:05:06.12", "1:35.47", "2.42" """;

		var ts = TrackTimeStamp.ParseCsvString(value);

		True(ts != null);
		True(ts.Start == TimeSpan.Parse("0:03:30.65"));
		True(ts.End == TimeSpan.Parse("0:05:06.12"));
		True(ts.Duration == TimeSpan.Parse("0:01:35.47"));
		True(ts.SilenceDuration.TotalSeconds == double.Parse("2.42"));
	}
}
