using System.Collections.Generic;

using Mp3Slap.General;
using Mp3Slap.Test;

namespace Test;

public class TTimeStampParseTests : SilenceDetectBase
{
	public bool WriteParsedLogs = false;

	[Fact]
	public void PARSE1()
	{
		string ln = "0:05:47.50, 0:09:39.59, 0:03:52.08, zz0:05:47.80, 0:09:36.13, 0:03:48.33, 3.76, 0.30";

		TTimeStamp itm = new();

		True(itm.ParseRawLine(ln));

		bool pass = itm.Init(
			prev: null,
			next: null,
			totalDuration: TimeSpan.FromHours(1),
			padDef: TimeSpan.FromSeconds(0.3),
			out TrackTimeStamp stamp);

		True(pass);

		//SilDetTimeStampsCSVParser cparser = new();
		//cparser.Parse(csvText, combineCuts: false, fixMetaCountToNew: false);
	}

	[Fact]
	public void PARSE_Short()
	{
		string csvText = csvLog_gen_short();

		SilDetTimeStampsCSVParser cparser = new();
		cparser.Parse(csvText, combineCuts: false, fixMetaCountToNew: false);
	}

	[Fact]
	public void PARSE2()
	{
		string ln = "0:05:47.50, 0:09:39.59, 0:03:52.08, zz0:05:47.80, 0:09:36.13, 0:03:48.33, 3.76, 0.30";

		SilDetTimeStampsCSVParser parser = new();
		//parser.Parse(



		TTimeStamp itm = new();

		True(itm.ParseRawLine(ln));

		bool pass = itm.Init(
			prev: null,
			next: null,
			totalDuration: TimeSpan.FromHours(1),
			padDef: TimeSpan.FromSeconds(0.3),
			out TrackTimeStamp stamp);

		True(pass);

		//SilDetTimeStampsCSVParser cparser = new();
		//cparser.Parse(csvText, combineCuts: false, fixMetaCountToNew: false);
	}


	public static List<TrackTimeStamp> GetStamps1()
		=> WriterTests1.GetStamps1();

}
