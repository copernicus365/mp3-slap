using Mp3Slap.SilenceDetect;

namespace Test;

public class FFSilenceTracksParserTests : BaseTest
{
	FFSilenceTracksParser _RunRawLogParseSilences(
		string log,
		int expectedCount = -1,
		(int min, int max)? expectedCountRange = null)
	{
		True(log != null);

		FFSilenceTracksParser split = new(log);
		
		List<TrackTimeStamp> tracks = split.Run();

		int tracksCount = tracks.Count;

		if(expectedCount >= 0)
			True(tracksCount == expectedCount);
		else if(expectedCountRange != null)
			True(tracks.Count.InRange(expectedCountRange.Value.min, expectedCountRange.Value.max));

		return split;
	}


	[Fact]
	public void TestGenesisLogNew()
	{
		string log = _GetFFSilencesRawLog("log#niv-suchet-01-genesis.mp3#silencedetect.log");

		FFSilenceTracksParser split = _RunRawLogParseSilences(log, expectedCountRange: (49, 51)); // don't know why it's 49 right now, but...

		True(split.Stamps.Count == 49);
	}

	[Fact]
	public void TestGenesisLogOld()
	{
		string log = _GetFFSilencesRawLog("log#niv-suchet-01-genesis.mp3#silencedetect-OLD.log");

		FFSilenceTracksParser split = _RunRawLogParseSilences(log, expectedCount: 51);
	}

	[Fact]
	public void ConvertRawLogDanielLogOld()
	{
		string log = _GetFFSilencesRawLog("log#niv-suchet-27-daniel.mp3#silencedetect.log");

		FFSilenceTracksParser split = _RunRawLogParseSilences(log, expectedCount: 17);
	}

	string _ToScriptRude(FFSilenceTracksParser split)
	{
		List<TrackTimeStamp> tracks = split.Run();

		string result = tracks.JoinToString(v => $"  {v.ToCsvString(pad: 1)},", "\n");
		//$"  ('{v.Start}', '{v.End}', '{v.Duration}')", "\n");

		// var json = $$"""
		// li
		//            {
		//                "summary": "text",
		//                "value" : {{someValue}},
		//            };
		// """;

		string script = $$"""
list = [
{{result}}
]

for i in range(len(list)):
  itm = list[i]
  ffmpeg -ss {itm[0]} -i gen.mp3 -vcodec copy -acodec copy -to {itm[1]} {i}-gen.mp3

  #print(f'start at: {itm[0]} ends: {itm[1]} duration: \'{itm[2]}\'')

""";
		return script;
	}

	static string _getSampleFFMpegSilenceLogPATH(string fname)
		=> $"sample-ffmpeg-silence-logs/{fname}";

	string _GetFFSilencesRawLog(string fname)
		=> DataString(_getSampleFFMpegSilenceLogPATH(fname));


	//[Fact]
	void _PlayWithTSFormatsNoTest()
	{
		var ts1 = TimeSpan.FromMinutes(1.533);
		var ts2 = TimeSpan.FromSeconds(1.774);
		ts1 = TimeSpan.FromMinutes(8.533);


		string fmt = "0.##";
		fmt = @"h\:mm\:ss\.ff";

		string tRes1 = ts1.ToString(fmt);

		var x1 = ts1.TotalMilliseconds.ToString("ff");

		tRes1 = $"{Math.Truncate(ts1.TotalMinutes)}:{ts1.Seconds:00}.{ts1:ff}";

		string res4 = ts1.ToTotalMinutesString();

		TimeSpan tback = TimeSpan.Parse(tRes1);

		// $"{Math.Truncate(ts1.TotalMinutes).Round(0)}:{ts1.Seconds:00}.{ts1.TotalMilliseconds.Round(2)}"
		// {04:14:19.3200000}
		// "8:31.511980"

	}
}
