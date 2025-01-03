using System.Diagnostics.CodeAnalysis;
using System.Text;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

/// <summary>
/// Writes and parses silence detect list to / from CSVs.
/// </summary>
public class SilDetTimeStampsCSVWriter
{
	public int Count => Stamps?.Count ?? 0;

	public SilDetTimeStampsMeta Meta { get; set; }

	public List<TrackTimeStamp> Stamps { get; set; }

	public void InitForWrite(
		double pad,
		double sdDuration,
		List<TrackTimeStamp> stamps,
		FFAudioMeta ffmeta,
		string filePath = null)
	{
		Stamps = stamps;

		string fileName = filePath.IsNulle() ? null : Path.GetFileName(filePath);
		Meta = new SilDetTimeStampsMeta(
			count: Count,
			sdDuration: 0,
			pad: pad,
			fileName,
			filePath,
			ffmeta);
	}

	public void InitForWrite(
		List<TrackTimeStamp> stamps,
		SilDetTimeStampsMeta meta)
	{
		Stamps = stamps;
		Meta = meta with { count = stamps.Count };
	}

	public void FixMetaCountIfNeeded()
	{
		FFAudioMeta meta = Meta?.meta;
		if(Meta != null && Meta.count != Count)
			Meta = Meta with { count = Count };
	}


	public string WriteToString(
		bool includeCSVHeader = true)
	{
		StringBuilder sb = new();

		string json = GetHeaderJSON();
		sb.AppendLine($"# {json}");

		if(includeCSVHeader)
			sb.AppendLine(TrackTimeStamp.CSVHeader);

		for(int i = 0; i < Stamps.Count; i++) {
			TrackTimeStamp st = Stamps[i];
			string res = st.ToCsvString();
			sb.AppendLine(res);
		}
		sb.AppendLine();

		string rep = sb.ToString();
		return rep;
	}

	string GetHeaderJSON()
		=> Meta.ToJson(includeDefaults: false, indent: false, camelCase: true);

	void SetMetaHeaderFromJSON(string txt)
		=> Meta = txt.DeserializeJson<SilDetTimeStampsMeta>();


	public bool CombineCuts()
	{
		List<TrackTimeStamp> _stamps = Stamps;
		SilDetTimeStampsCSVParser.CombineCuts(ref _stamps);
		Stamps = _stamps;

		FixMetaCountIfNeeded();
		return true; // even if somehow count is same (?), fact is some WERE marked as Cut
	}


	#region --- PARSE ---

	public List<TrackTimeStamp> Parse(string text, bool fixMetaCountToNew = true)
	{
		string[] lines = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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

		if(fixMetaCountToNew)
			FixMetaCountIfNeeded();

		return Stamps;
	}

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

	public static TrackTimeStamp ParseCsvString_OLD(string line)
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

	#endregion
}
