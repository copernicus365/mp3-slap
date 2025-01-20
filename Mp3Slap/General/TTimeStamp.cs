using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Mp3Slap.General;

public enum TStampType
{
	Default = 0,
	StartOnly,

}

public class TTimeStamp
{
	public TimeSpan Start;
	public TimeSpan End;
	public TimeSpan Duration;
	public TimeSpan SoundStart;
	public TimeSpan SoundEnd;
	public TimeSpan SoundDuration;
	public double SilenceDuration;
	public double Pad;
	public bool IsCut;
	public bool IsAdd;

	public bool IsValid;
	public string ErrorMsg;

	public int ColsCount { get; private set; }
	string[] arr;


	/// <summary>
	/// Purpose is to RAW parse directly from string a single CSV line,
	/// handling ALL possible / available input types (which have varying number
	/// of items). Goal is to NOT process or calculate extra values here besides
	/// what was entered, in large part because to calculate properly, we need the
	/// CONTEXT and before and after lines. So this type allows raw values recorded
	/// but further calculations happen after all have been parsed. We DO however
	/// set <see cref="ColsCount"/> so that key details is known.
	/// </summary>
	public bool ParseRawLine(string line)
	{
		line = line.NullIfEmptyTrimmed();
		if(line.IsNulle())
			return false;

		IsCut = line[0] == '-';
		IsAdd = line[0] == '+';
		if(IsCut || IsAdd)
			line = line[1..].NullIfEmptyTrimmed();

		arr = line
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(v => v?.Trim('"').NullIfEmptyTrimmed())
			.ToArray();

		int len = ColsCount = arr.Length;
		if(len != 8) {

			if(len == 1) {
				if(!TryParseDoubleSecsElseTSLenient(arr[0], out TimeSpan ts))
					return false;
				Start = ts;

				return IsValid = Start >= TimeSpan.Zero;
			}
			else if(len == 2) {

				(bool success, bool isTS, TimeSpan ts, double db) = TryParseDoubleSecsElseTSLenient(arr[0]);
				if(!success) //(!TryParseTSLenient(arr[0], out TimeSpan ts))
					return false;
				Start = ts;

				(success, isTS, ts, db) = TryParseDoubleSecsElseTSLenient(arr[1]);
				if(!success)
					return false;

				End = isTS
					? ts
					: ts += Start; // treating 2nd DOUBLE VALUE **as duration**

				return IsValid = Start >= TimeSpan.Zero && End > Start;
			}

			return false;
		}

		for(int i = 0; i < arr.Length; i++) {
			string val = arr[i];
			if(val == null)
				return false;

			if(i == 3 || i == 7) {
				double dVal = val.ToDouble(-2);
				if(dVal < -1)
					return false;

				switch(i) {
					case 3: Pad = dVal; break;
					case 7: SilenceDuration = dVal; break;
					default: return false;
				}
				continue;
			}

			if(!TryParseTSLenient(val, out TimeSpan ts))
				return false;

			switch(i) {
				case 0: Start = ts; break;
				case 1: End = ts; break;
				case 2: Duration = ts; break;
				case 4: SoundStart = ts; break;
				case 5: SoundEnd = ts; break;
				case 6: SoundDuration = ts; break;
				default: return false;
			}
		}

		if(SoundStart < TimeSpan.Zero || SoundEnd < SoundStart)
			return false;

		return IsValid = true;
	}

	public static (bool success, bool isTS, TimeSpan ts, double db) TryParseDoubleSecsElseTSLenient(string s)
	{
		if(s.IndexOf(':') > 0) {
			return TryParseTSLenient(s, out TimeSpan ts)
				? (true, true, ts, default)
				: (false, true, default, default);
		}

		if(double.TryParse(s, out double dval)) {
			TimeSpan ts = TimeSpan.FromSeconds(dval);
			return (true, false, ts, dval);
		}

		return default;
	}


	public static bool TryParseDoubleSecsElseTSLenient([NotNullWhen(true)] string s, out TimeSpan result)
	{
		if(s.IndexOf(':') > 0)
			return TryParseTSLenient(s, out result);

		if(double.TryParse(s, out double dval)) {
			result = TimeSpan.FromSeconds(dval);
			return true;
		}

		result = default;
		return false;
	}

	public static bool TryParseTSLenient([NotNullWhen(true)] string s, out TimeSpan result)
	{
		int colonCnt = s.Count(c => c == ':');
		if(colonCnt == 1)
			s = "0:" + s;

		if(TimeSpan.TryParse(s, null, out result))
			return true;

		return false;
	}


	public bool Init(
		TTimeStamp prev,
		TTimeStamp next,
		TimeSpan totalDuration,
		TimeSpan padDef,
		out TrackTimeStamp stamp)
	{
		stamp = null;

		bool isFirst = prev == null;
		bool isEnd = next == null;
		TimeSpan nextStart = isEnd ? totalDuration : next.Start;

		TimeSpan pad = Pad > 0 ? TimeSpan.FromSeconds(Pad) : padDef;

		switch(ColsCount) {
			case 1: {
				//Start = isFirst ? TimeSpan.Zero : prev.End;
				End = nextStart;
				Duration = End - Start;
				if(Duration <= TimeSpan.Zero)
					return SetError("1: Negative duration");
				stamp = new TrackTimeStamp() {
					Start = Start,
					End = End,
					Duration = Duration,
					Pad = pad,
					IsAdd = IsAdd,
					IsCut = IsCut,
				};
				return true;
			}
			case 2: {
				// NOTE: Though it DID have 2 cols, 2nd one must have been 0 / 00:00:00

				//// I think REQUIRE End to be set
				if(End <= TimeSpan.Zero) {
					if(!isEnd)
						return SetError("2: No End");
					End = nextStart;
				}
				Duration = End - Start;

				if(Duration <= TimeSpan.Zero)
					return SetError("1: Negative duration");

				stamp = new TrackTimeStamp() {
					Start = Start,
					End = End,
					Duration = Duration,
					Pad = pad,
					IsAdd = IsAdd,
					IsCut = IsCut,
				};
				return true;
			}
			case 8: {
				if(!isFirst) {
					if(Start == default || SoundStart == default)
						return SetError("8: starts not set");
				}
				if(!isEnd) {
					if(End == default || SoundEnd == default)
						return SetError("8: ends not set");
				}

				if(Duration == default && End != default)
					Duration = End - Start;
				if(SoundDuration == default && SoundEnd != default)
					SoundDuration = SoundEnd - SoundStart;

				stamp = new TrackTimeStamp() {
					Start = Start,
					End = End,
					Pad = pad,
					Duration = Duration,
					SoundStart = SoundStart,
					SoundEnd = SoundEnd,
					SoundDuration = SoundDuration,
					IsAdd = IsAdd,
					IsCut = IsCut,
					SilenceDuration = default
				};
				return true;
			}
			default: throw new ArgumentOutOfRangeException(nameof(ColsCount));
		}
	}

	public bool SetError(string msg)
	{
		ErrorMsg = msg;
		return IsValid = false;
	}


	public override string ToString() => ToCsvString();

	public string ToCsvString()
		=> $"{Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}{(IsAdd ? ", -add" : null)}{(IsCut ? ", -cut" : null)}{(IsValid ? "" : $" -invalid {ErrorMsg}")}";


	public const string TSFrmt = @"h\:mm\:ss\.ff";



	//public TrackTimeStamp ToTrackTimeStamp()
	//{
	//	throw new NotImplementedException();
	//	//TrackTimeStamp stamp = new(SoundStart, SoundEnd, SoundDuration, SilenceDuration, Pad) { IsCut = IsCut };
	//	//TrackTimeStamp stamp = new(SoundStart, SoundEnd, dur, silence, pad) { IsCut = isCut };
	//	//return stamp;
	//}
}
