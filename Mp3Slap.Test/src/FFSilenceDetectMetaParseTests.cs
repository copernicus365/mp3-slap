using Mp3Slap.SilenceDetect;

namespace Test;

public class FFSilenceDetectMetaParseTests : SilenceDetectBase
{
	[Fact]
	public void Test1()
	{
		string log = GetSampleFFSilLog("log#gen1_79#silencedetect.log");
		
		FFSilenceDetectMetaParse mp = new();
		True(mp.Parse(log));

		FFAudioMeta m = mp.Meta;

		True(m.source.EndsWith("/NIV-Suchet-01-Genesis.mp3"));
		True(m.album == "NIV Suchet (01) Genesis");
		True(m.artist == "Moses, Prophets, Jesus/David Suchet");
		True(m.album_artist == "Moses, Prophets, Jesus/David Suchet");
		True(m.encoder == "Lavf58.29.100");
		True(m.title == "NIV Suchet (01) Genesis");
		True(m.track == "01/1");
		True(m.genre == "Bible");
		True(m.date == "2014");
		True(m.start == 0.025056);
		True(m.bitrate == "128 kb/s");
	}
}
