namespace Test;

public class ParseCsvTests : BaseTest
{
	[Fact]
	public void TestParseCuts1()
	{
		//string csvText = GetDataDirPath("sample-results/results-dan3.csv");
		string csvText = DataString("sample-results/results-dan1.csv");

		TrackTimeStampsCsv csv = new();
		csv.Parse(csvText);
		csv.CombineCuts();

		True(csv.Valid);

		string result = csv.Write();

		string writePath = GetDataDirPath("sample-results/results-dan4.csv");
		File.WriteAllText(writePath, result);
	}


	[Fact]
	public void ParseTupString1()
	{
		string value = """ "0:03:30.65", "0:05:06.12", "01:35.47", "2.42", "0:01:35.47" """;

		var ts = TrackTimeStamp.ParseCsvString(value);

		True(ts != null && ts.Start == TimeSpan.Parse("0:03:30.65"));
	}
}
