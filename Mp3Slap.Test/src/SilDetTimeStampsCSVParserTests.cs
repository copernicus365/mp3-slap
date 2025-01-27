using Mp3Slap.General;

namespace Test;

public class SilDetTimeStampsCSVParserTests : SilenceDetectBase
{
	public bool WriteParsedLogs = false;

	[Fact]
	public void PARSE_Short()
		=> go1(csvLog_gen_short(), 5, 5);

	[Fact]
	public void Parse_1NumCsv_1()
	{
		SilDetTimeStampsCSVParser cparser = go1(csvLog_1num_v1(), 17, 14, finalExpectedArr: @"
0:00:00.00, 0:05:47.50, 0:05:47.50, 
0:05:47.50, 0:09:39.59, 0:03:52.09, 
0:09:39.59, 0:13:53.59, 0:04:14.00, 
0:13:53.59, 0:18:11.87, 0:04:18.28, 
0:18:11.87, 0:22:06.05, 0:03:54.18, 
0:22:06.05, 0:25:36.95, 0:03:30.90, 
0:25:36.95, 0:29:29.98, 0:03:53.03, 
0:29:29.98, 0:33:02.25, 0:03:32.27, 
0:33:02.25, 0:37:37.82, 0:04:35.57, 
0:37:37.82, 0:41:30.11, 0:03:52.29, 
0:41:30.11, 0:43:30.00, 0:01:59.89, 
0:43:30.00, 0:45:45.41, 0:02:15.41, 
0:45:45.41, 0:50:00.00, 0:04:14.59, 
0:50:00.00, 4:04:09.81, 3:14:09.81, 
");
		TTimeStamp tt = cparser.Stamps[10].ToTT();
		TTimeStamp exp = new("41:30.11", "43:30") { Pad = 0.3 };
		True(tt == exp);
	}

	[Fact]
	public void Parse_2NumCsv_1()
		=> go1(csvLog_2num_v1(), 10, 10);

	[Fact]
	public void Parse_2NumCsv_Adds_Cuts_Simpl1()
		=> go1(csvLog_2num_adds_cuts_simple1(), 8, 7, @"
00:00, 02:00
02:00, 03:00
03:00, 04:00
04:00, 05:00
05:00, 05:30
05:30, 07:00
09:00, 10:00");

	[Fact]
	public void Parse_2NumCsv_Adds_Cuts_Simpl2()
		=> go1(csvLog_2num_adds_cuts_simple2(), 6, 6);

	[Fact]
	public void ParseLargeCsv_WithSubs_Gen()
		=> go1(csvLog_gen_subs(), 54, 50);

	SilDetTimeStampsCSVParser go1(string csvText, int expCount, int expCountAfterCuts, string finalExpectedArr = null, bool finIgnoreCuts = true, bool finClearPad = true)
	{
		SilDetTimeStampsCSVParser cparser = new();
		cparser.Parse(csvText, combineCuts: false, fixMetaCountToNew: false);

		True(cparser.Count == expCount);

		bool countShouldChange = expCount != expCountAfterCuts;

		True(cparser.CombineCuts());
		cparser.FixMetaCountIfNeeded();

		if(expCountAfterCuts >= 0) {
			True(cparser.Count == expCountAfterCuts);
			if(cparser.Meta != null)
				True(cparser.Meta.count == expCountAfterCuts);
		}

		if(finalExpectedArr != null) {
			TTimeStamp[] arrEx = TTimeStamp.ParseFromFullString(finalExpectedArr);
			TTimeStamp[] parsedArr = cparser.Stamps.Select(s => s.ToTT(ignoreCuts: finIgnoreCuts, clearPad: finClearPad)).ToArray();

			True(arrEx.Length == parsedArr.Length);
			for(int i = 0; i < parsedArr.Length; i++)
				True(arrEx[i] == parsedArr[i]);
		}

		bool writeTempCsv = false;

		if(writeTempCsv) {
			SilDetTimeStampsCSVWriter csv = cparser.ToCsv();

			bool gHeader = true;
			bool gShort = true;
			string result = csv.WriteToString(includeCSVHeader: gHeader, getShortCsv: gShort);

			string writePath = GetDataDirPath($"{SampleResultsDirName}/write-temp/temp-general.csv");

			//if(WriteParsedLogs)
			File.WriteAllText(writePath, result);
		}
		return cparser;
	}
}
