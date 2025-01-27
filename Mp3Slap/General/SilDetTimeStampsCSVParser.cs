using System.Diagnostics.CodeAnalysis;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

/// <summary>
/// PARSES CSV silence detect file. Just for parsing, not writing.
/// It's really good to have a separate PARSER, because parsing / reading allows tons of
/// extra scenarios, like SUBTRACTIONS or ADDS, etc.
/// </summary>
public class SilDetTimeStampsCSVParser
{
	public double PadDefault { get; set; } = FFSDLogToTimeStampsParser.PadDefault;

	public int Count => Stamps?.Count ?? 0;

	public SilDetTimeStampsMeta Meta { get; set; }

	public List<TrackTimeStamp> Stamps { get => _stamps; set => _stamps = value; }

	public SResult Result { get; set; }

	List<TrackTimeStamp> _stamps;
	List<TTimeStamp> _tstamps;

	public void FixMetaCountIfNeeded()
	{
		var meta = Meta?.meta;
		if(Meta != null && Meta.count != Count) {
			Meta = Meta with { count = Count };

			if(Meta.pad > 0)
				PadDefault = Meta.pad;
		}
	}

	string[] lines;

	public void Parse(
		string text,
		bool combineCuts = true,
		bool fixMetaCountToNew = true)
	{
		lines = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if(lines.IsNulle()) {
			setError("Empty lines");
			return;
		}

		string fline = lines[0];
		if(fline[0] == '#' && fline.Contains('{')) {
			fline = fline[1..];
			SetMetaHeaderFromJSON(fline);
		}

		_tstamps ??= new(lines.Length);

		TimeSpan padDef = TimeSpan.FromSeconds(PadDefault);

		for(int i = 0; i < lines.Length; i++) {
			string ln = lines[i];
			if(ln.IsNulle())
				continue;

			if(ln[0] == '#') // skip first line header meta JSON if there
				continue;

			if(i < 2 && ln.StartsWith("Start")) // skip CSV header if there
				continue;

			TTimeStamp stampNw = new();
			if(!stampNw.ParseRawLine(ln))
				continue;

			_tstamps.Add(stampNw);
		}

		int lastI = _tstamps.Count - 1;

		Stamps = new List<TrackTimeStamp>(_tstamps.Count);

		for(int i = 0; i < _tstamps.Count; i++) {
			TTimeStamp itm = _tstamps[i];

			bool pass = itm.Init(
				prev: i < 1 ? null : _tstamps[i - 1],
				next: i == lastI ? null : _tstamps[i + 1],
				totalDuration: Meta?.meta?.duration ?? TimeSpan.Zero,
				padDef: padDef,
				out TrackTimeStamp stamp);

			if(!pass) {
				setError($"Fail to parse stamp: {itm.ErrorMsg}");
				return;
			}

			Stamps.Add(stamp);
		}

		if(combineCuts) {
			CombineCuts();

			if(fixMetaCountToNew)
				FixMetaCountIfNeeded();
		}

		Result = new(true);
	}

	void setError(string msg)
		=> Result = new SResult(false, msg);

	public SilDetTimeStampsCSVWriter ToCsv()
	{
		SilDetTimeStampsCSVWriter csv = new() {
			Stamps = Stamps,
			Meta = Meta,
		};
		return csv;
	}

	void SetMetaHeaderFromJSON(string txt)
		=> Meta = txt.DeserializeJson<SilDetTimeStampsMeta>();

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

	public bool CombineCuts() => CombineCuts(ref _stamps);

	public static bool CombineCuts(ref List<TrackTimeStamp> stamps)
	{
		if(stamps.IsNulle())
			return false;

		bool hasSubs = !stamps.None(s => s.IsCut);
		bool hasAdds = !stamps.None(s => s.IsAdd);

		if(!hasSubs && !hasAdds)
			return false;

		if(hasSubs) {
			TrackTimeStamp[] arr = [.. stamps];

			if(arr[0].IsCut)
				arr[0].IsCut = false; // can't be valid i any case, but logic below depends on this

			int negIndexSince = 0;

			for(int i = arr.Length - 1; i >= 0; i--) {
				TrackTimeStamp st = arr[i];

				bool hasPrevNegatives = negIndexSince > 0;

				if(st.IsCut) {
					if(!hasPrevNegatives)
						negIndexSince = i;
					continue;
				}

				if(!hasPrevNegatives)
					continue;

				TrackTimeStamp lastCutSt = arr[negIndexSince]; // "last" = ASC order. if 3,4,5 are cuts, last = [5]

				st.End = lastCutSt.End;
				st.SoundEnd = lastCutSt.SoundEnd;
				st.SetDurations();

				negIndexSince = 0;
			}

			stamps = arr.Where(st => !st.IsCut).ToList();
		}

		if(hasAdds && stamps.NotNulle()) {

			TrackTimeStamp[] arr = [.. stamps];

			// LOOK AHEAD +1 LOOP, IF NEXT ISADD, if needed it's Start time cuts down current item's start time
			// arr.Length - 1 --> IGNORE final IsAdd, it already was calculated on 2nd to last look ahead
			for(int i = 0; i < arr.Length - 1; i++) {
				TrackTimeStamp nxt = arr[i + 1];

				if(!nxt.IsAdd)
					continue;

				TrackTimeStamp st = arr[i];

				if(st.End <= nxt.Start)
					continue;

				st.End = nxt.Start;
				st.SoundStart = nxt.SoundStart; // TOO COMPLEX !! **MUST** JUST OVERWRITE for KISS!! There's no valid way Start/End changes and the sound-start etc doesn't
				st.SetDurations();
			}

			stamps = arr.Where(st => !st.IsCut).ToList();
		}


		return true; // even if somehow count is same (?), fact is some WERE marked as Cut

		//FixMetaCountIfNeeded();
	}

}
