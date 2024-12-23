using System.Diagnostics.CodeAnalysis;

namespace Mp3Slap.General;

public class TrackTimeStamp
{
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

		SetSoundStartEndPads(pad: pad);
	}

	public TrackTimeStamp(TimeSpan startSound, TimeSpan endSound, TimeSpan duration, double silenceDuration, double pad)
	{
		if(startSound < TimeSpan.Zero || endSound < startSound)
			throw new ArgumentOutOfRangeException();

		SoundStart = startSound;
		SoundEnd = endSound;

		ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);
		if(duration == TimeSpan.Zero)
			duration = SoundEnd - startSound; // old note, maybe not relevant now: // do not calculate! start/end could be padded results

		SoundDuration = duration;

		SilenceDuration = TimeSpan.FromSeconds(silenceDuration);

		SetSoundStartEndPads(pad: TimeSpan.FromSeconds(pad));
	}

	/// <summary>FOR START / END times directly, NOT based on start / end of sound.</summary>
	public TrackTimeStamp(TimeSpan start, TimeSpan end, TimeSpan pad = default)
	{
		if(start < TimeSpan.Zero || end < start || pad < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException();

		Start = start;
		End  = end;
		Duration = End - Start;
		//SilenceDuration = silenceDuration ?? TimeSpan.Zero;

		SetSoundStartEndPads(pad: pad);
	}



	public TimeSpan Start { get; private set; }

	public TimeSpan End { get; private set; }

	public TimeSpan Duration { get; private set; }

	public TimeSpan SoundStart { get; private set; }

	public TimeSpan SoundEnd { get; private set; }

	public TimeSpan SoundDuration;

	public TimeSpan SilenceDuration { get; private set; }

	public TimeSpan Pad { get; private set; }

	public bool IsCut { get; set; }

	public bool IsAdd { get; set; }

	public bool SetSoundStartEndPads(TimeSpan pad, TimeSpan? previousEndSilence = null)
	{
		if(pad <= TimeSpan.Zero || previousEndSilence != null && previousEndSilence.Value <= TimeSpan.Zero)
			goto NOPAD;

		Pad = pad;
		Start = SoundStart - pad;
		End = SoundEnd + SilenceDuration - pad;
		Duration = End - Start;

		if(Start < TimeSpan.Zero || End <= SoundStart)
			goto NOPAD;

		return true;

	NOPAD:
		Pad = TimeSpan.Zero;
		Start = SoundStart;
		End = SoundEnd + SilenceDuration;
		Duration = End - Start;

		return false;
	}


	public const string TSFrmt = @"h\:mm\:ss\.ff";

	public override string ToString() => ToCsvString();

	/// <summary>
	/// Used for our unique parse and write formats, so do not
	/// alter w/out breaking change consideration.
	/// </summary>
	public string ToCsvString()
		=> $"{SoundStart.ToString(TSFrmt)}, {SoundEnd.ToString(TSFrmt)}, {SoundDuration.ToString(TSFrmt)}, {SilenceDuration.TotalSeconds:0.00}, {Pad.TotalSeconds:0.00}, {Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}, {Duration.ToString(TSFrmt)}, ";


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
		TrackTimeStamp cStamp = new(SoundStart, last.SoundEnd, silenceDuration: last.SilenceDuration, Pad);
		return cStamp;
	}
}

public static class TrackTimeStampX
{
	public static void SetSoundStartEndPads(this IList<TrackTimeStamp> stamps, double pad)
	{
		if(stamps.IsNulle() || stamps.Count < 2)
			return;

		TimeSpan padTS = TimeSpan.FromSeconds(pad);

		TrackTimeStamp last = stamps[0];

		for(int i = 0; i < stamps.Count; i++) {
			TrackTimeStamp s = stamps[i];
			bool failed = s.SetSoundStartEndPads(padTS, i == 0 ? TimeSpan.Zero : last.SilenceDuration);
			last = s;
		}
	}
}
