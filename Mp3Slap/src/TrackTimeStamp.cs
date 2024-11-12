using System.Diagnostics.CodeAnalysis;

namespace Mp3Slap;

public class TrackTimeStamp
{
	public TrackTimeStamp(TimeSpan start, TimeSpan end, TimeSpan duration, double silenceDuration)
	{
		if(start < TimeSpan.Zero || end < start)
			throw new ArgumentOutOfRangeException();

		Start = start;
		End = end;
		Duration = duration; // do not calculate! start/end could be padded results
		SilenceDuration = TimeSpan.FromSeconds(silenceDuration);
	}

	public TrackTimeStamp(double start, double end, double? silenceDuration = null)
	{
		if(start < 0 || end < start || silenceDuration < 0)
			throw new ArgumentOutOfRangeException();

		Start = TimeSpan.FromSeconds(start);
		End = TimeSpan.FromSeconds(end);
		Duration = End - Start;
		SilenceDuration = TimeSpan.FromSeconds(silenceDuration ?? 0);
	}

	public TimeSpan Start { get; private set; }

	public TimeSpan End { get; private set; }

	public TimeSpan Duration;

	public TimeSpan SilenceDuration { get; private set; }

	public bool IsCut { get; set; }

	public bool Empty => Duration <= TimeSpan.Zero;

	public const string Format1 = @"h\:mm\:ss\.ff";

	public override string ToString() => ToCsvString(pad: 0);

	/// <summary>
	/// Used for our unique parse and write formats, so do not
	/// alter w/out breaking change consideration.
	/// </summary>
	public string ToCsvString(double pad)
		=> $"{add(Start, -pad).ToString(Format1)}, {add(End, pad).ToString(Format1)}, {Duration.ToString(Format1)}, {SilenceDuration.TotalSeconds:0.00}";
	//{Duration.ToTotalMinutesString2()}

	TimeSpan add(TimeSpan ts, double pad)
		=> ts.Add(TimeSpan.FromSeconds(pad)).Max(TimeSpan.Zero);


	public static TrackTimeStamp ParseCsvString(string line)
	{
		line = line.NullIfEmptyTrimmed();
		if(line == null || line.Length < 4) return null;

		bool isCut = line[0] == '-';
		if(isCut)
			line = line[1..].NullIfEmptyTrimmed();

		string[] arr = line
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(v => v?.Trim('"').NullIfEmptyTrimmed())
			.ToArray();

		if(arr.Length != 4)
			return null;

		TimeSpan start = default;
		TimeSpan end = default;
		TimeSpan dur = default;
		double silence = default;

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

			if(!TryParseTSLenient(val, out TimeSpan ts))
				return null;
			//if(!TimeSpan.TryParse(val, out TimeSpan ts))
			//	return null;

			switch(i) {
				case 0: start = ts; break;
				case 1: end = ts; break;
				case 2: dur = ts; break;
				default: return null;
			}
		}

		if(start < TimeSpan.Zero || end < start)
			return null;

		return new TrackTimeStamp(start, end, dur, silence) { IsCut = isCut };
	}

	public static bool TryParseTSLenient([NotNullWhen(true)] string? s, out TimeSpan result)
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

	public void CombineFollowingCuts(Span<TrackTimeStamp> arr)
	{
		if(arr.Length < 1)
			return;

		TrackTimeStamp last = arr[arr.Length - 1];

		End = last.End;
		SilenceDuration = last.SilenceDuration;

		for(int i = 0; i < arr.Length; i++) {
			Duration += arr[i].Duration;
		}
	}
}
