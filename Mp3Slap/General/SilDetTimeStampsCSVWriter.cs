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
		bool includeCSVHeader = true,
		bool getShortCsv = false)
	{
		StringBuilder sb = new();

		string json = GetHeaderJSON();
		sb.AppendLine($"# {json}");

		if(includeCSVHeader)
			sb.AppendLine(TrackTimeStamp.GetHeader(getShortCsv));

		for(int i = 0; i < Stamps.Count; i++) {
			TrackTimeStamp st = Stamps[i];
			string res = getShortCsv
				? st.ToShortCsvString()
				: st.ToCsvString();
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
}
