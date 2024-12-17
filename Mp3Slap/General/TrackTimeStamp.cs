using System.Diagnostics.CodeAnalysis;

namespace Mp3Slap.General;

public class TrackTimeStamp
{
	/// <summary>
	/// For FFMpeg's silence detect script that has 3 main args per silence detected, as doubles / decimals.
	/// </summary>
	public TrackTimeStamp(double start, double end, double? silenceDuration = null, double pad = 0)
		: this(
			  start: TimeSpan.FromSeconds(start),
			  end: TimeSpan.FromSeconds(end),
			  silenceDuration: silenceDuration == null ? null : TimeSpan.FromSeconds(silenceDuration.Value),
			  pad: TimeSpan.FromSeconds(pad))
	{ }

	public TrackTimeStamp(TimeSpan start, TimeSpan end, TimeSpan? silenceDuration = null, TimeSpan pad = default)
	{
		if(start < TimeSpan.Zero || end < start || silenceDuration < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException();

		Start = start;
		End = end;
		Duration = End - Start;
		SilenceDuration = silenceDuration ?? TimeSpan.Zero;

		SetPads(pad: pad);
	}

	public TrackTimeStamp(TimeSpan start, TimeSpan end, TimeSpan duration, double silenceDuration, double paddedDuration)
	{
		if(start < TimeSpan.Zero || end < start)
			throw new ArgumentOutOfRangeException();

		Start = start;
		End = end;

		ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);
		if(duration == TimeSpan.Zero)
			duration = End - start; // old note, maybe not relevant now: // do not calculate! start/end could be padded results

		Duration = duration;

		SilenceDuration = TimeSpan.FromSeconds(silenceDuration);

		SetPads(TimeSpan.FromSeconds(paddedDuration));
	}

	public TimeSpan Start { get; private set; }

	public TimeSpan End { get; private set; }

	public TimeSpan Duration;

	public TimeSpan SilenceDuration { get; private set; }

	public TimeSpan PaddedStart { get; private set; }

	public TimeSpan PaddedEnd { get; private set; }

	public TimeSpan PaddedDuration { get; private set; }

	public TimeSpan Pad { get; private set; }

	public bool IsCut { get; set; }

	public bool Empty => Duration <= TimeSpan.Zero;


	public bool SetPads(TimeSpan pad, TimeSpan? previousEndSilence = null)
	{
		if(pad <= TimeSpan.Zero || previousEndSilence != null && previousEndSilence.Value <= TimeSpan.Zero)
			goto NOPAD;

		Pad = pad;
		PaddedStart = Start - pad;
		PaddedEnd = End + SilenceDuration - pad;
		PaddedDuration = PaddedEnd - PaddedStart;

		if(PaddedStart < TimeSpan.Zero || PaddedEnd <= Start)
			goto NOPAD;

		return true;

	NOPAD:
		Pad = TimeSpan.Zero;
		PaddedStart = Start;
		PaddedEnd = End + SilenceDuration;
		PaddedDuration = PaddedEnd - PaddedStart;

		return false;
	}


	public const string TSFrmt = @"h\:mm\:ss\.ff";

	public override string ToString() => ToCsvString();

	/// <summary>
	/// Used for our unique parse and write formats, so do not
	/// alter w/out breaking change consideration.
	/// </summary>
	public string ToCsvString()
		=> $"{Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}, {Duration.ToString(TSFrmt)}, {SilenceDuration.TotalSeconds:0.00}, {Pad.TotalSeconds:0.00}, {PaddedStart.ToString(TSFrmt)}, {PaddedEnd.ToString(TSFrmt)}, {PaddedDuration.ToString(TSFrmt)}, ";

	//public string ToCsvString2()
	//	=> $"\"{Start.ToString(TSFrmt)}\", \"{End.ToString(TSFrmt)}\", \"{Duration.ToString(TSFrmt)}\", \"{SilenceDuration.TotalSeconds:0.00}\", \"{Pad.TotalSeconds:0.00}\", \"{PaddedStart.ToString(TSFrmt)}\", \"{PaddedEnd.ToString(TSFrmt)}\", \"{PaddedDuration.ToString(TSFrmt)}\", ";


	public static TrackTimeStamp ParseCsvString(string line)
	{
		line = line.NullIfEmptyTrimmed();
		if(line == null || line.Length < 8) return null;

		bool isCut = line[0] == '-';
		if(isCut)
			line = line[1..].NullIfEmptyTrimmed();

		string[] arr = line
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(v => v?.Trim('"').NullIfEmptyTrimmed())
			.ToArray();

		if(arr.Length != 8)
			return null;

		TimeSpan start = default;
		TimeSpan end = default;
		TimeSpan dur = default;
		double silence = default;
		double pad = default;
		TimeSpan pdstart = default;
		TimeSpan pdend = default;
		TimeSpan pddur = default;

		for(int i = 0; i < arr.Length; i++) {
			string val = arr[i];
			if(val == null)
				return null;

			if(i == 3) {
				silence = val.ToDouble(-2);
				if(silence < -1)
					return null;
				continue;
			}
			else if(i == 4) {
				pad = val.ToDouble(-2);
				continue;
			}

			if(!TryParseTSLenient(val, out TimeSpan ts))
				return null;
			//if(!TimeSpan.TryParse(val, out TimeSpan ts))
			//	return null;

			switch(i) {
				case 0: start = ts; break;
				case 1: end = ts; break;
				case 2: dur = ts; break;
				case 5: pdstart = ts; break;
				case 6: pdend = ts; break;
				case 7: pddur = ts; break;
				default: return null;
			}
		}

		if(start < TimeSpan.Zero || end < start)
			return null;

		TrackTimeStamp stamp = new(start, end, dur, silence, pad) { IsCut = isCut };
		return stamp;
	}

	public static bool TryParseTSLenient([NotNullWhen(true)] string s, out TimeSpan result)
	{
		if(TimeSpan.TryParse(s, null, out result))
			return true;

		if(s.Count(c => c == ':') == 1) {
			s = "0:" + s;
			if(TimeSpan.TryParse(s, null, out result))
				return true;
		}

		return false;
	}

	public TrackTimeStamp CombineCutsFromLastToNew(TrackTimeStamp last)
	{
		TrackTimeStamp cStamp = new(Start, last.End, silenceDuration: last.SilenceDuration, Pad);
		return cStamp;
	}

	public TrackTimeStamp CombineFollowingCuts(Span<TrackTimeStamp> arr)
	{
		if(arr.Length < 1)
			throw new ArgumentNullException(nameof(arr));

		TrackTimeStamp last = arr[^1]; // arr[arr.Length - 1];

		//End = last.End;
		//SilenceDuration = last.SilenceDuration;

		//for(int i = 0; i < arr.Length; i++) {
		//	Duration += arr[i].Duration;
		//}

		TrackTimeStamp cStamp = new(Start, last.End, silenceDuration: last.SilenceDuration, Pad);
		return cStamp;
	}
}

public static class TrackTimeStampX
{
	public static void SetPads(this IList<TrackTimeStamp> stamps, double pad)
	{
		if(stamps.IsNulle() || stamps.Count < 2)
			return;

		TimeSpan padTS = TimeSpan.FromSeconds(pad);

		TrackTimeStamp last = stamps[0];

		for(int i = 0; i < stamps.Count; i++) {
			TrackTimeStamp s = stamps[i];
			bool failed = s.SetPads(padTS, i == 0 ? TimeSpan.Zero : last.SilenceDuration);
			last = s;
		}
	}
}
