using System.Text;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

/// <summary>
/// Writes and parses silence detect list to / from CSVs.
/// </summary>
public class SilDetTimeStampsCSV
{
	public int Count => Stamps?.Count ?? 0;

	public SilDetTimeStampsMeta Meta { get; set; }

	public List<TrackTimeStamp> Stamps { get; set; }

	public void InitForWrite(
		List<TrackTimeStamp> stamps,
		SilDetTimeStampsMeta meta)
	{
		Stamps = stamps;
		Meta = meta with { count = stamps.Count };
	}

	public void FixMetaCountIfNeeded()
	{
		var meta = Meta?.meta;
		if(Meta != null && Meta.count != Count)
			Meta = Meta with { count = Count };
	}

	public void InitForWrite(
		List<TrackTimeStamp> stamps,
		double duration,
		double pad,
		string fileName,
		string filePath,
		FFAudioMeta meta = null)
	{
		Stamps = stamps;

		if(fileName.IsNullOrHasNone() && filePath.NotNulle()) {
			fileName = Path.GetFileName(filePath);
		}
		else {
			fileName ??= "";
			filePath ??= Path.GetFileName(fileName);
		}

		Meta = new SilDetTimeStampsMeta(
			count: Stamps.Count,
			duration: duration,
			pad: pad,
			fileName: fileName,
			filePath: filePath,
			meta: meta);
	}

	public string WriteToString()
	{
		StringBuilder sb = new();

		string x = GetHeaderJSON();
		sb.AppendLine($"# {x}");

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

			var stamp = TrackTimeStamp.ParseCsvString(ln);
			if(stamp == null)
				continue;

			Stamps.Add(stamp);
		}

		if(fixMetaCountToNew)
			FixMetaCountIfNeeded();

		return Stamps;
	}

	public bool CombineCuts()
	{
		var stamps = Stamps.ToArray();

		if(stamps.IsNulle())
			return false;

		if(stamps.None(s => s.IsCut))
			return false;

		if(stamps[0].IsCut)
			stamps[0].IsCut = false; // can't be valid i any case, but logic below depends on this

		int negIndexSince = 0;

		for(int i = stamps.Length - 1; i >= 0; i--) {
			TrackTimeStamp st = stamps[i];

			bool hasPrevNegatives = negIndexSince > 0;

			if(st.IsCut) {
				if(!hasPrevNegatives)
					negIndexSince = i;
				continue;
			}

			if(!hasPrevNegatives)
				continue;

			TrackTimeStamp lastCutSt = stamps[negIndexSince]; // "last" = ASC order. if 3,4,5 are cuts, last = [5]

			TrackTimeStamp cStamp = st.CombineCutsFromLastToNew(lastCutSt);
			stamps[i] = cStamp;

			negIndexSince = 0;
		}

		Stamps = stamps.Where(st => !st.IsCut).ToList();

		FixMetaCountIfNeeded();

		return true; // even if somehow count is same (?), fact is some WERE marked as Cut
	}
}
