using System.Diagnostics.CodeAnalysis;
using System.Text;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

/// <summary>
/// PARSES CSV silence detect file. Just for parsing, not writing.
/// That's very good because parsing allows tons of extra cases, SUBTRACTIONS or ADDS, etc.
/// </summary>
public class SilDetTimeStampsCSVParser
{
	public int Count => Stamps?.Count ?? 0;

	public SilDetTimeStampsMeta Meta { get; set; }

	public List<TrackTimeStamp> Stamps { get; set; }

	public void FixMetaCountIfNeeded()
	{
		var meta = Meta?.meta;
		if(Meta != null && Meta.count != Count)
			Meta = Meta with { count = Count };
	}

	string[] lines;

	public List<TrackTimeStamp> Parse(string text, bool fixMetaCountToNew = true)
	{
		lines = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if(lines.IsNulle())
			return null;

		string fline = lines[0];
		if(fline[0] == '#' && fline.Contains('{')) {
			fline = fline[1..];
			SetMetaHeaderFromJSON(fline);
		}

		Stamps ??= new(lines.Length);

		for(int i = 0; i < lines.Length; i++) {
			string ln = lines[i];
			if(ln[0] == '#')
				continue;

			TrackTimeStamp stamp = ParseCsvString(ln);
			if(stamp == null)
				continue;

			Stamps.Add(stamp);
		}

		//if(fixMetaCountToNew)
		//	FixMetaCountIfNeeded();

		return Stamps;
	}

	void SetMetaHeaderFromJSON(string txt)
		=> Meta = txt.DeserializeJson<SilDetTimeStampsMeta>();

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

		TTimeStamp st = new();
	
		for(int i = 0; i < arr.Length; i++) {
			string val = arr[i];
			if(val == null)
				return null;

			if(i == 3) {
				st.SilenceDuration = val.ToDouble(-2);
				if(st.SilenceDuration < -1)
					return null;
				continue;
			}
			else if(i == 4) {
				st.Pad = val.ToDouble(-2);
				continue;
			}

			if(!TryParseTSLenient(val, out TimeSpan ts))
				return null;
			//if(!TimeSpan.TryParse(val, out TimeSpan ts))
			//	return null;

			switch(i) {
				case 0: st.SoundStart = ts; break;
				case 1: st.SoundEnd = ts; break;
				case 2: st.SoundDuration = ts; break;
				case 5: st.Start = ts; break;
				case 6: st.End = ts; break;
				case 7: st.Duration = ts; break;
				default: return null;
			}
		}

		if(st.SoundStart < TimeSpan.Zero || st.SoundEnd < st.SoundStart)
			return null;

		TrackTimeStamp stamp = new(st.SoundStart, st.SoundEnd, st.SoundDuration, st.SilenceDuration, st.Pad) { IsCut = isCut };
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

	public int ColsCount { get; private set; }
	string[] arr;

	public bool Parse(string line)
	{
		line = line.NullIfEmptyTrimmed();
		if(line == null || line.Length < 2)
			return false;

		IsCut = line[0] == '-';
		if(IsCut)
			line = line[1..].NullIfEmptyTrimmed();

		arr = line
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(v => v?.Trim('"').NullIfEmptyTrimmed())
			.ToArray();

		int len = ColsCount = arr.Length;
		if(len != 8) {

			if(len == 1) {
				if(!TryParseTSLenient(arr[0], out TimeSpan ts))
					return false;
				Start = ts;

				return Start >= TimeSpan.Zero;
			}
			else if(len == 2) {
				if(!TryParseTSLenient(arr[0], out TimeSpan ts))
					return false;
				Start = ts;

				if(TryParseTSLenient(arr[1], out ts))
					End = ts;
				else {
					double dVal = arr[1].ToDouble(-2);
					if(dVal <= 0)
						return false;

					// but is this seconds? minutes? so no allow??
					// however, only one that makes sense of a true decimal is
					// seconds, eg 4.413 in minutes?! but 4.413 in seconds, fine
					ts = TimeSpan.FromSeconds(dVal);
				}
				return Start >= TimeSpan.Zero && End > Start;
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

		return true;
		//TrackTimeStamp stamp = new(SoundStart, SoundEnd, SoundDuration, SilenceDuration, Pad) { IsCut = IsCut };
		//return stamp;
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

}