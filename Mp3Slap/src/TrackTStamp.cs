namespace Mp3Slap;

public class TrackTStamp
{
	public TrackTStamp(double start, double end, double? silenceDuration = null)
	{
		if(start < 0 || end < start || silenceDuration < 0)
			throw new ArgumentOutOfRangeException();

		Start = TimeSpan.FromSeconds(start);
		End = TimeSpan.FromSeconds(end);
		Duration = End - Start;
		SilenceDuration = TimeSpan.FromSeconds(silenceDuration ?? 0);
	}

	public TimeSpan Start { get; }

	public TimeSpan End { get; }

	public TimeSpan Duration;

	public TimeSpan SilenceDuration { get; }

	public bool Empty => Duration <= TimeSpan.Zero;

	public const string Format1 = @"h\:mm\:ss\.ff";

	public override string ToString()
		=> $"('{Start.ToString(Format1)}', '{End.ToString(Format1)}', '{Duration:mm\\:ss}', '{SilenceDuration.TotalSeconds:0.00}')";
		//=> $"{Start.ToString(Format1)} - {End.ToString(Format1)} (dur: {Duration:mm\\:ss} silence: {SilenceDuration.TotalSeconds:0.00})";

	public string GetString(double pad)
		=> $"('{add(Start, -pad).ToString(Format1)}', '{add(End, pad).ToString(Format1)}', '{Duration:mm\\:ss}', '{SilenceDuration.TotalSeconds:0.00}')";

	TimeSpan add(TimeSpan ts, double pad)
		=> ts.Add(TimeSpan.FromSeconds(pad)).Max(TimeSpan.Zero);
}
