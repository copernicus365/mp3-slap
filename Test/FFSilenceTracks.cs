namespace Test;

public class FFSilenceTracksT : BaseTest
{
	[Fact]
	public void Test1()
	{
		string log = DataString("log1-genesis.txt");
		True(log != null);

		FFSilenceTracksParser split = new(log);

		List<TrackTStamp> tracks = split.Run();

		True(tracks.Count.InRange(44, 52));


		string result = tracks.JoinToString(v => $"  {v.GetString(1)},", "\n");
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
}
