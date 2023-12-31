namespace Test;

public class FFSilenceTracksTests : BaseTest
{
	void hhh()
	{
		var items = new (string start, string end, string duration, string silenceDur)[] {
			("0:00:00.00", "0:05:45.01", "05:44", "3.79"),
			("0:05:46.80", "0:09:37.13", "03:48", "3.76")
		};
	}

	[Fact]//
	public void Test1()
	{
		string log = DataString("log1-genesis.txt");

		string extLogPath = "/Users/nikos/repos/praxis/mp3-split/messiah/log1.txt"; // use when experimenting live running repeatedly
		log = File.ReadAllText(extLogPath);

		True(log != null);

		FFSilenceTracksParser split = new(log);

		List<TrackTimeStamp> tracks = split.Run();

		True(tracks.Count.InRange(44, 52));

		string result = tracks.JoinToString(v => $"  {v.ToCsvString(1)},", "\n");
		//$"  ('{v.Start}', '{v.End}', '{v.Duration}')", "\n");

		// var json = $$"""
		// li
		//            {
		//                "summary": "text",
		//                "value" : {{someValue}},
		//            };
		// """;

		string resScript = $$"""
list = [
{{result}}
]

for i in range(len(list)):
  itm = list[i]
  ffmpeg -ss {itm[0]} -i gen.mp3 -vcodec copy -acodec copy -to {itm[1]} {i}-gen.mp3

  #print(f'start at: {itm[0]} ends: {itm[1]} duration: \'{itm[2]}\'')

""";

	}

	[Fact]
	public void TestLiveLog()
	{
		// use when experimenting live running repeatedly
		string extLogPath = "/Users/nikos/repos/praxis/mp3-split/messiah/log2.txt";

		string log = File.ReadAllText(extLogPath);

		True(log != null);

		FFSilenceTracksParser split = new(log);

		List<TrackTimeStamp> tracks = split.Run();

		TrackTimeStampsCsv csv = new() {
			Name = "Daniel",
			SrcPath = "/Users/nikos/repos/praxis/mp3-split/messiah/NIV-Suchet-27-Daniel.mp3",
			LogPath = extLogPath,
			Pad = 0,
			Stamps = tracks
		};

		string result = csv.Write();

		string writePath = GetDataDirPath("sample-results/results-dan1.csv");
		File.WriteAllText(writePath, result);
	}



	[Fact]
	public void TestTSFormats()
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
