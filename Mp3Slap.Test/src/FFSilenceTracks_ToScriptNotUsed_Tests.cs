using Mp3Slap.General;
using Mp3Slap.SilenceDetect;

namespace Test;

/// <summary>
/// Keep... but not used, ideas (like python script instead of stupid shell?)
/// a mess tho
/// </summary>
public class FFSilenceTracks_ToScriptNotUsed_Tests : FFSilenceDetectBase
{
	[Fact]
	public void TestWriteSplitScript()
	{
		FFSDLogToTimeStampsParser split = new FFSDLogToTimeStampsParserTests().ParseFFSDLog(
			GetSampleFFSilLog("log#niv-suchet-01-genesis.mp3#silencedetect.log"),
			expectedCountRange: (49, 51)); // don't know why it's 49 right now, but...

		string script = _ToScriptRude2(split, "gen.mp3", "gen-{i}.mp3");
	}

	string _ToScriptRude(FFSDLogToTimeStampsParser split)
	{
		List<TrackTimeStamp> tracks = split.Parse();

		string result = tracks.JoinToString(v => $"  {v.ToCsvString()},", "\n");
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

	string _ToScriptRude2(FFSDLogToTimeStampsParser split, string srcMp3, string destMp3Templ)
	{
		List<TrackTimeStamp> tracks = split.Parse();

		string result = tracks.JoinToString(v => $"  {v.ToCsvString()},", "\n");
		//$"  ('{v.Start}', '{v.End}', '{v.Duration}')", "\n");

		List<string> vals = new(tracks.Count);

		for(int i = 0; i < tracks.Count; i++) {
			TrackTimeStamp t = tracks[i];
			string dest = destMp3Templ.Replace("{i}", i.ToString("00"));

			string scr = $"ffmpeg -ss '{t.Start.TotalSeconds}' -i \'{srcMp3}\' -vcodec copy -acodec copy -to '{t.End.TotalSeconds}' {dest}";
			vals.Add(scr);
		}

		string scriptF = vals.JoinToString("\n");
		return scriptF;

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
