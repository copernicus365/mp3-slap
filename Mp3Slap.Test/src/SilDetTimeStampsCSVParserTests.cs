using Mp3Slap.General;

namespace Test;

public class SilDetTimeStampsCSVParserTests : SilenceDetectBase
{
	public bool WriteParsedLogs = false;

	[Fact]
	public void PARSE_Short()
		=> go1(csvLog_gen_short(), 5, 5, out SilDetTimeStampsCSVParser cparser);

	[Fact]
	public void Parse_1NumCsv_1()
	{
		go1(csvLog_1num_v1(), 17, 14, out SilDetTimeStampsCSVParser cparser);
		TTimeStamp tt = cparser.Stamps[10].ToTT();
		TTimeStamp exp = new TTimeStamp("41:30.11", "43:30") { Pad = 0.3 };
		True(tt == exp);
	}

	[Fact]
	public void Parse_2NumCsv_1()
		=> go1(csvLog_2num_v1(), 54, 50);

	[Fact]
	public void Parse_2NumCsv_Adds_Cuts_Simpl1()
		=> go1(csvLog_2num_adds_cuts_simple1(), 8, 7, out _);

	[Fact]
	public void Parse_2NumCsv_Adds_Cuts_Simpl2()
		=> go1(csvLog_2num_adds_cuts_simple2(), 6, 6);

	[Fact]
	public void ParseLargeCsv_WithSubs_Gen()
		=> go1(csvLog_gen_subs(), 54, 50);

	[Fact]
	public void ParseLargeCsvDoc1_Gen()
		=> go1(csvLog_gen_subs(), 51, 50);

	void go1(string csvText, int expCount, int expCountAfterCuts)
		=> go1(csvText, expCount, expCountAfterCuts, out _);

	void go1(string csvText, int expCount, int expCountAfterCuts, out SilDetTimeStampsCSVParser cparser)
	{
		cparser = new();
		cparser.Parse(csvText, combineCuts: false, fixMetaCountToNew: false);

		True(cparser.Count == expCount);

		bool countShouldChange = expCount != expCountAfterCuts;

		True(cparser.CombineCuts() == countShouldChange);
		cparser.FixMetaCountIfNeeded();

		if(expCountAfterCuts >= 0) {
			True(cparser.Count == expCountAfterCuts && cparser.Meta.count == expCountAfterCuts);
		}

		bool writeTempCsv = false;

		if(writeTempCsv) {
			SilDetTimeStampsCSVWriter csv = cparser.ToCsv();

			string result = csv.WriteToString();

			string writePath = GetDataDirPath($"{SampleResultsDirName}/write-temp/temp-general.csv");

			//if(WriteParsedLogs)
			File.WriteAllText(writePath, result);
		}
	}
}
