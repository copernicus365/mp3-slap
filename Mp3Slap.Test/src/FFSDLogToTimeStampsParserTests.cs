using Mp3Slap.General;
using Mp3Slap.SilenceDetect;

namespace Test;

public class FFSDLogToTimeStampsParserTests : FFSilenceDetectBase
{
	[Fact]
	public void GenLog()
		=> _RunParse_BasicCount_Test("log#niv-suchet-01-genesis.mp3#silencedetect.log", 49);

	[Fact]
	public void GenOlderLog()
		=> _RunParse_BasicCount_Test("log#niv-suchet-01-genesis.mp3#silencedetect-OLD.log", 51);

	[Fact]
	public void DanLog()
		=> _RunParse_BasicCount_Test("log#niv-suchet-27-daniel.mp3#silencedetect.log", 17);

	void _RunParse_BasicCount_Test(string logName, int expectedCount)
	{
		FFSDLogToTimeStampsParser split = ParseFFSDLog(
			GetSampleFFSilLog(logName),
			expectedCount: expectedCount);
	}

	public FFSDLogToTimeStampsParser ParseFFSDLog(
		string log,
		int expectedCount = -1,
		(int min, int max)? expectedCountRange = null)
	{
		True(log != null);
		
		FFSDLogToTimeStampsParser parser = new(log);

		List<TrackTimeStamp> tracks = parser.Parse();

		int tracksCount = tracks.Count;

		if(expectedCount >= 0)
			True(tracksCount == expectedCount);
		else if(expectedCountRange != null)
			True(tracks.Count.InRange(expectedCountRange.Value.min, expectedCountRange.Value.max));

		return parser;
	}
}
