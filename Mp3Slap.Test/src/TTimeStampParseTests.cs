using Mp3Slap.General;

namespace Test;

public class TTimeStampParseTests : SilenceDetectBase
{
	public bool WriteParsedLogs = false;

	[Fact]
	public void FAIL_BadDoubleVal()
		=> fail("0:05:47.50, 0:09:39.59, 0:03:52.08, zz0:05:47.80, 0:09:36.13, 0:03:48.33, 3.76, 0.30");

	[Fact]
	public void FAIL_DoubleIsTS()
		=> fail("0:05:47.50, 0:09:39.59, 0:03:52.08, 0:05:47.80, 0:09:36.13, 0:03:48.33, 3.76, 0.30");

	void fail(string ln)
	{
		TTimeStamp itm = new();

		False(itm.ParseRawLine(ln));
		True(itm.ErrorMsg.NotNulle());
	}

	[Fact]
	public void PARSE1()
	{
		string ln = "0:05:47.50, 0:09:39.59, 0:03:52.08, 0.30, 0:05:47.80, 0:09:36.13, 0:03:48.33, 3.76";

		TTimeStamp itm = new();

		True(itm.ParseRawLine(ln));

		TTimeStamp exp = new("0:05:47.50", "0:09:39.59", "0:03:52.08", "0.30", "0:05:47.80", "0:09:36.13", "0:03:48.33", "3.76");
		//"0:09:39.59", "0:13:53.59", "0:04:13.99", "0.30", "0:09:39.89", "0:13:49.81", "0:04:09.92", "4.07"
		True(itm == exp);

		bool pass = itm.Init(
			prev: null,
			next: null,
			totalDuration: TimeSpan.FromHours(1),
			padDef: TimeSpan.FromSeconds(0.3),
			out TrackTimeStamp stamp);

		True(pass);
	}

	[Fact]
	public void PARSE1_StartEnd()
	{
		string ln = "0:05:47.50, 0:09:39.59";

		TTimeStamp itm = new();

		True(itm.ParseRawLine(ln));

		TTimeStamp exp = new("0:05:47.50", "0:09:39.59");

		True(itm == exp);

		bool pass = itm.Init(
			prev: null,
			next: null,
			totalDuration: TimeSpan.FromHours(1),
			padDef: TimeSpan.FromSeconds(0.3),
			out TrackTimeStamp stamp);

		True(pass);
	}

	// ---

	[Fact]
	public void PARSE1_ManualCheckToo()
	{
		string ln = "0:09:39.59, 0:13:53.59, 0:04:13.99, 0.30, 0:09:39.89, 0:13:49.81, 0:04:09.92, 4.07";

		TTimeStamp ts = new();

		True(ts.ParseRawLine(ln));

		TTimeStamp exp = new("0:09:39.59", "0:13:53.59", "0:04:13.99", "0.30", "0:09:39.89", "0:13:49.81", "0:04:09.92", "4.07");
		True(ts == exp);

		bool pass = ts.Init(
			prev: null,
			next: null,
			totalDuration: TimeSpan.FromHours(1),
			padDef: TimeSpan.FromSeconds(0.3),
			out TrackTimeStamp stamp);

		True(pass);

		True(ts != null);
		True(ts.Start == TimeSpan.Parse("0:09:39.59"));
		True(ts.End == TimeSpan.Parse("0:13:53.59")); // changed from .59 -> .58, just ignore
		True(ts.Duration == TimeSpan.Parse("0:04:13.99"));
		True(ts.Pad == double.Parse("0.30"));
		True(ts.SoundStart == TimeSpan.Parse("0:09:39.89"));
		True(ts.SoundEnd == TimeSpan.Parse("0:13:49.81"));
		True(ts.SoundDuration == TimeSpan.Parse("0:04:09.92"));
		True(ts.SilenceDuration == double.Parse("4.07"));
	}
}
