using Mp3Slap.General;
using Mp3Slap.SilenceDetect;

namespace Test;

public class FFSilenceTracksParserTests : FFSilenceDetectBase
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
		FFSilenceDetToTimeStampsParser split = ParseFFSDLog(
			GetSampleFFSilLog(logName),
			expectedCount: expectedCount);
	}

	public FFSilenceDetToTimeStampsParser ParseFFSDLog(
		string log,
		int expectedCount = -1,
		(int min, int max)? expectedCountRange = null)
	{
		True(log != null);
		// ParseFFSDLog
		FFSilenceDetToTimeStampsParser split = new(log);

		List<TrackTimeStamp> tracks = split.Parse();

		int tracksCount = tracks.Count;

		if(expectedCount >= 0)
			True(tracksCount == expectedCount);
		else if(expectedCountRange != null)
			True(tracks.Count.InRange(expectedCountRange.Value.min, expectedCountRange.Value.max));

		return split;
	}
}
