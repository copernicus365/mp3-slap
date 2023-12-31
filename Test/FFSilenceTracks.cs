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

		string result = tracks.JoinToString(v => $"- {v}", "\n");
    }
}
