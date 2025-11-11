using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Mp3Slap.General;

public enum TStampType
{
	Default = 0,
	StartOnly,

}

public class TTimeStamp : IComparable<TTimeStamp>, IComparer<TTimeStamp>, IEquatable<TTimeStamp>
{
	public TimeSpan Start;
	public TimeSpan End;
	public TimeSpan Duration;
	public double Pad;
	public TimeSpan SoundStart;
	public TimeSpan SoundEnd;
	public TimeSpan SoundDuration;
	public double SilenceDuration;
	public bool IsCut;
	public bool IsAdd;


	public TTimeStamp() { }

	public TTimeStamp(params string[] arr)
	{
		if(arr.IsNulle())
			return;

		IsValid = true;
		int len = arr.Length;

		switch(len) {
			case 1:
				Start = ParseDoubleSecsElseTSLenient_DefOnNull(arr[0]);
				// setDurations - no end so not poss
				return;
			case 2:
				Start = ParseDoubleSecsElseTSLenient_DefOnNull(arr[0]);
				End = ParseDoubleSecsElseTSLenient_DefOnNull(arr[1]);
				setDurations();
				return;
			case 3:
				Start = ParseDoubleSecsElseTSLenient_DefOnNull(arr[0]);
				End = ParseDoubleSecsElseTSLenient_DefOnNull(arr[1]);
				Duration = ParseDoubleSecsElseTSLenient_DefOnNull(arr[2]);
				return;
			case 8:
				Start = ParseDoubleSecsElseTSLenient_DefOnNull(arr[0]);
				End = ParseDoubleSecsElseTSLenient_DefOnNull(arr[1]);
				Duration = ParseDoubleSecsElseTSLenient_DefOnNull(arr[2]);
				Pad = double.Parse(arr[3]);
				SoundStart = ParseDoubleSecsElseTSLenient_DefOnNull(arr[4]);
				SoundEnd = ParseDoubleSecsElseTSLenient_DefOnNull(arr[5]);
				SoundDuration = ParseDoubleSecsElseTSLenient_DefOnNull(arr[6]);
				SilenceDuration = double.Parse(arr[7]);
				return;
			default: throw new ArgumentOutOfRangeException();
		}
	}

	/// <summary>Mostly just for diagnostics, so priv for now...</summary>
	void setDurations()
	{
		if(End > TimeSpan.Zero)
			Duration = End - Start;
		if(SoundEnd > TimeSpan.Zero)
			SoundDuration = SoundEnd - SoundStart;
	}

	public int Compare(TTimeStamp x, TTimeStamp y)
	{
		if(x is null || y is null) return 1;

		int comp;

		if(!comp1(x.Start, y.Start, out comp)) return comp;
		if(!comp1(x.End, y.End, out comp)) return comp;
		if(!comp1(x.Duration, y.Duration, out comp)) return comp;
		if(!comp1(x.SoundStart, y.SoundStart, out comp)) return comp;
		if(!comp1(x.SoundEnd, y.SoundEnd, out comp)) return comp;
		if(!comp1(x.SoundDuration, y.SoundDuration, out comp)) return comp;
		if(!comp1(x.SilenceDuration, y.SilenceDuration, out comp)) return comp;
		if(!comp1(x.Pad, y.Pad, out comp)) return comp;
		if(!comp1(x.IsCut, y.IsCut, out comp)) return comp;
		if(!comp1(x.IsAdd, y.IsAdd, out comp)) return comp;
		return 0;
	}

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
			return setError("Null or empty");

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
				string val = arr[0];

				if(!TryParseDoubleSecsElseTSLenient(val, out TimeSpan ts))
					return setError($"1: invalid time `{val}`");
				Start = ts;

				return setErrorIfInvalidDuration(Start < TimeSpan.Zero);
			}
			else if(len == 2) {

				string val = arr[0];
				(bool success, bool isTS, TimeSpan ts, double db) = TryParseDoubleSecsElseTSLenient(val);
				if(!success)
					return setError($"2a: invalid time `{val}`");
				Start = ts;

				val = arr[1];
				(success, isTS, ts, db) = TryParseDoubleSecsElseTSLenient(val);
				if(!success)
					return setError($"2b: invalid time `{val}`");

				End = isTS
					? ts
					: ts += Start; // treating 2nd DOUBLE VALUE **as duration**

				Duration = End - Start; // <-- Ensure Duration is set for 2-col case

				return setErrorIfInvalidDuration(Start < TimeSpan.Zero || End <= Start);
			}

			return setError($"{len}: invalid count of line items");
		}

		for(int i = 0; i < arr.Length; i++) {
			string val = arr[i];
			if(val == null)
				return setError("8: null/empty");

			if(i == 3 || i == 7) {
				double dVal = val.ToDouble(-2);
				if(dVal < -1)
					return setError($"8 double: negative num `{val}`");

				switch(i) {
					case 3: Pad = dVal; break;
					case 7: SilenceDuration = dVal; break;
					default: throw new ArgumentOutOfRangeException();
				}
				continue;
			}

			if(!TryParseTSLenient(val, out TimeSpan ts))
				return setError($"8: invalid time `{val}`");

			switch(i) {
				case 0: Start = ts; break;
				case 1: End = ts; break;
				case 2: Duration = ts; break;
				case 4: SoundStart = ts; break;
				case 5: SoundEnd = ts; break;
				case 6: SoundDuration = ts; break;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		return setErrorIfInvalidDuration(SoundStart < TimeSpan.Zero || SoundEnd < SoundStart);
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

				if(isEnd) {
					if(totalDuration <= TimeSpan.Zero)
						return setError("1: Last item can't be a single (start) time if total duration wasn't specified somewhere otherwise. In that case last item must have 2 times, ie: [start, end]");
				}
				else if(next.IsAdd) {

				}

				//if(IsAdd)
				//	return setError("1: Adds can't be a singleton / only start time, must have an end..."); // just simplify things...

				//Start = isFirst ? TimeSpan.Zero : prev.End;
				End = nextStart;
				Duration = End - Start;
				if(Duration <= TimeSpan.Zero)
					return setError("1: Negative duration");

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
						return setError("2: No End");
					End = nextStart;
				}
				Duration = End - Start;

				if(Duration <= TimeSpan.Zero)
					return setError("1: Negative duration");

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
						return setError("8: starts not set");
				}
				if(!isEnd) {
					if(End == default || SoundEnd == default)
						return setError("8: ends not set");
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

	public static TimeSpan ParseDoubleSecsElseTSLenient_DefOnNull([NotNullWhen(true)] string s)
	{
		if(s.IsNulle())
			return default;
		if(!TryParseDoubleSecsElseTSLenient(s, out TimeSpan result))
			throw new ArgumentException();
		return result;
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


	bool setError(string msg)
	{
		ErrorMsg = msg;
		return IsValid = false;
	}


	bool setErrorIfInvalidDuration(bool error = true)
	{
		if(error)
			ErrorMsg = "Invalid duration";
		return IsValid = !error;
	}


	public override string ToString() => ToCsvString();

	public string ToCsvString()
		=> $"{Start.ToString(TSFrmt)}, {End.ToString(TSFrmt)}{(IsAdd ? ", -add" : null)}{(IsCut ? ", -cut" : null)}{(IsValid ? "" : $" -invalid {ErrorMsg}")}";

	public int CompareTo(TTimeStamp other)
		=> Compare(this, other);

	public bool Equals(TTimeStamp other)
		=> Compare(this, other) == 0;

	public const string TSFrmt = @"h\:mm\:ss\.ff";


	#region -- Equals etc boiler --

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool comp1(TimeSpan t1, TimeSpan t2, out int comp)
	{
		comp = t1.CompareTo(t2);
		return comp == 0;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool comp1(double t1, double t2, out int comp)
	{
		comp = t1.CompareTo(t2);
		return comp == 0;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool comp1(bool t1, bool t2, out int comp)
	{
		comp = t1.CompareTo(t2);
		return comp == 0;
	}

	public override bool Equals(object obj)
	{
		if(ReferenceEquals(this, obj))
			return true;

		if(obj is null)
			return false;

		return Equals(obj as TTimeStamp);
	}

	public override int GetHashCode() => throw new NotImplementedException();

	public static bool operator ==(TTimeStamp left, TTimeStamp right)
	{
		if(left is null) {
			return right is null;
		}

		int comp = left.CompareTo(right);
		return comp == 0;
	}

	public static bool operator !=(TTimeStamp left, TTimeStamp right) => !(left == right);

	public static bool operator <(TTimeStamp left, TTimeStamp right) => left is null ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;

	public static bool operator <=(TTimeStamp left, TTimeStamp right) => left is null || left.CompareTo(right) <= 0;

	public static bool operator >(TTimeStamp left, TTimeStamp right) => left is not null && left.CompareTo(right) > 0;

	public static bool operator >=(TTimeStamp left, TTimeStamp right) => left is null ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;

	#endregion


	public static TTimeStamp[] ParseFromFullString(string input, double pad = 0)
	{
		TTimeStamp[] arr = input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(ln => {
				if(ln.StartsWith('#'))
					return null;

				string[] vals = ln.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				if(vals.IsNulle())
					return null;

				TTimeStamp stamp = new(vals);
				if(pad > 0 && stamp.Pad == 0 && stamp.ColsCount < 3)
					stamp.Pad = pad;

				return stamp;
			})
			.ToArray();
		return arr;
	}

}
