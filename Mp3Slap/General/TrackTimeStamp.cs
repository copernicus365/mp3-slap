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

	/// <summary>
	/// For easy testing purposes...
	/// </summary>
	/// <param name="arr"></param>
	public TrackTimeStamp(params string[] arr)
		: this(ts(arr[0]), ts(arr[1]), TimeSpan.FromSeconds(arr[2].ToDouble()), TimeSpan.FromSeconds(0.3))
	{ }

	static TimeSpan ts(string v) => TimeSpan.Parse(v);

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
	public const string CSVHeaderShort = "Start,End,Duration,";

	public static string GetHeader(bool forShort) => forShort ? CSVHeaderShort : CSVHeader;

	public void SetDurations()
	{
		if(End > TimeSpan.Zero)
			Duration = End - Start;
	}

	public TimeSpan Start { get; set; }

	public TimeSpan End { get; set; }

	public TimeSpan Duration { get; set; }

	public TimeSpan Pad { get; set; }

	public TimeSpan SoundStart { get; set; }

	public TimeSpan SoundEnd { get; set; }

	public TimeSpan SoundDuration { get; set; }

	public TimeSpan SilenceDuration { get; set; }

	public bool IsCut { get; set; }

	public bool IsAdd { get; set; }

	/// <summary>
	/// Just for diagnostic purposes, TT has extra iequatables etc to make easy comparisons...
	/// </summary>
	public TTimeStamp ToTT(bool ignoreCuts = false, bool clearPad = false)
	{
		TTimeStamp tt = new() {
			Start = Start,
			End = End,
			Duration = Duration,
			Pad = Pad.TotalSeconds,
			SoundStart = SoundStart,
			SoundEnd = SoundEnd,
			SoundDuration = SoundDuration,
			SilenceDuration = SilenceDuration.TotalSeconds,
			IsCut = IsCut,
			IsAdd = IsAdd,
			IsValid = true
		};

		if(ignoreCuts)
			tt.IsCut = tt.IsAdd = false;

		if(clearPad)
			tt.Pad = 0;

		return tt;
	}

	public const string TSFrmt = @"h\:mm\:ss\.ff";

	public override string ToString() => ToCsvString();

	//public string ToCsvString(bool getShort = false)
	//	=> ToCsvStringNEW();


	public string ToShortCsvString()
		=> $"{Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}, {Duration.ToString(TSFrmt)}, ";

	public string ToCsvString()
		=> $"{Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}, {Duration.ToString(TSFrmt)}, {Pad.TotalSeconds:0.00}, {SoundStart.ToString(TSFrmt)}, {SoundEnd.ToString(TSFrmt)}, {SoundDuration.ToString(TSFrmt)}, {SilenceDuration.TotalSeconds:0.00}, ";
}

//public static bool USENEWCSV = true;
//
//public string ToCsvStringOLD() => $"{SoundStart.ToString(TSFrmt)}, {SoundEnd.ToString(TSFrmt)}, {SoundDuration.ToString(TSFrmt)}, {SilenceDuration.TotalSeconds:0.00}, {Pad.TotalSeconds:0.00}, {Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}, {Duration.ToString(TSFrmt)}, ";
