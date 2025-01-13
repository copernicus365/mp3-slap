namespace Mp3Slap.General;

public class TrackTimeStamp
{
	public TrackTimeStamp() { }

	/// <summary>
	/// For setting  based on start/end of SOUND times. Such as when feeding in from
	/// ffmpeg's silence detection points. But rarely would one want a track to begin
	/// at the microsecond in which the sound begins with no padding.
	/// </summary>
	public TrackTimeStamp(double startSound, double endSound, double? silenceDuration = null, double pad = 0)
		: this(
			  startSound: TimeSpan.FromSeconds(startSound),
			  endSound: TimeSpan.FromSeconds(endSound),
			  silenceDuration: silenceDuration == null ? null : TimeSpan.FromSeconds(silenceDuration.Value),
			  pad: TimeSpan.FromSeconds(pad))
	{ }

	/// <summary>Ibid.</summary>
	public TrackTimeStamp(TimeSpan startSound, TimeSpan endSound, TimeSpan? silenceDuration = null, TimeSpan pad = default)
	{
		if(startSound < TimeSpan.Zero || endSound < startSound || silenceDuration < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException();

		SoundStart = startSound;
		SoundEnd = endSound;
		SoundDuration = SoundEnd - SoundStart;
		SilenceDuration = silenceDuration ?? TimeSpan.Zero;

		if(pad <= TimeSpan.Zero)
			goto NOPAD;

		Pad = pad;
		Start = SoundStart - pad;
		End = SoundEnd + SilenceDuration - pad;
		Duration = End - Start;

		if(Start < TimeSpan.Zero || End <= SoundStart)
			goto NOPAD;

		return;

	NOPAD:
		Pad = TimeSpan.Zero;
		Start = SoundStart;
		End = SoundEnd + SilenceDuration;
		Duration = End - Start;
	}


	public const string CSVHeader = "Start,End,Duration,Pad,SoundStart,SoundEnd,SoundDuration,SilenceDuration,";

	public TimeSpan Start { get; init; }

	public TimeSpan End { get; init; }

	public TimeSpan Duration { get; init; }

	public TimeSpan Pad { get; init; }

	public TimeSpan SoundStart { get; init; }

	public TimeSpan SoundEnd { get; init; }

	public TimeSpan SoundDuration { get; init; }

	public TimeSpan SilenceDuration { get; init; }

	public bool IsCut { get; set; }

	public bool IsAdd { get; set; }


	public const string TSFrmt = @"h\:mm\:ss\.ff";

	public override string ToString() => ToCsvString();

	public string ToCsvString()
		=> USENEWCSV ? ToCsvStringNEW() : ToCsvStringOLD();

	public static bool USENEWCSV = true;

	public string ToCsvStringShort()
		=> $"{Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}, {Duration.ToString(TSFrmt)}, ";

	public string ToCsvStringNEW()
		=> $"{Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}, {Duration.ToString(TSFrmt)}, {Pad.TotalSeconds:0.00}, {SoundStart.ToString(TSFrmt)}, {SoundEnd.ToString(TSFrmt)}, {SoundDuration.ToString(TSFrmt)}, {SilenceDuration.TotalSeconds:0.00}, ";

	public string ToCsvStringOLD()
		=> $"{SoundStart.ToString(TSFrmt)}, {SoundEnd.ToString(TSFrmt)}, {SoundDuration.ToString(TSFrmt)}, {SilenceDuration.TotalSeconds:0.00}, {Pad.TotalSeconds:0.00}, {Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}, {Duration.ToString(TSFrmt)}, ";
}
