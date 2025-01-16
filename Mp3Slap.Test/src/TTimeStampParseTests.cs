using System.Collections.Generic;

using Mp3Slap.General;

namespace Test;

public class TTimeStampParseTests : BaseTest
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
}
