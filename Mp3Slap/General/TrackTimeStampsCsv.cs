using System.Text;
using System.Xml.Linq;

namespace Mp3Slap.General;

public class TrackTimeStampsCsv
{
	public string Name { get; set; }

	public string FileName { get; set; }

	public int Count => Stamps?.Count ?? 0;

	public double Pad { get; set; }

	public string SrcDir { get; set; }

	public string SrcPath { get; set; }

	public string LogPath { get; set; }

	public string CsvLogPath { get; set; }

	public List<TrackTimeStamp> Stamps { get; set; }

	public bool Valid { get; set; }

	public void Reset()
	{
		Name = SrcPath = LogPath = null;
		Pad = 0;
		Valid = false;
		Stamps = [];
	}

	public string Write()
	{
		StringBuilder sb = new();

		string x = GetHeaderXml();
		sb.AppendLine($"# {x}");

		for(int i = 0; i < Stamps.Count; i++) {
			TrackTimeStamp st = Stamps[i];
			string res = st.ToCsvString2(); // pad: Pad);
			sb.AppendLine(res);
		}
		sb.AppendLine();

		string rep = sb.ToString();
		return rep;
	}

	string GetHeaderXml()
	{
		// # <info name="dan" path="NIV-Suchet-27-Daniel.mp3" fflog="logs/NIV-Suchet-27-Daniel.mp3-silence-detect-parsed.txt" />
		string x = new XElement("info",
			new XAttribute("name", Name ?? ""),
			new XAttribute("pad", Pad),
			new XAttribute("count", Count),
			new XAttribute("src", SrcPath ?? ""),
			new XAttribute("log", LogPath ?? ""))
			.ToString(SaveOptions.DisableFormatting);
		return x;
	}

	void SetHeaderValsFromXml(string txt)
	{
		XElement x;
		try {
			x = XElement.Parse(txt);
			if(x == null || x.Name != "info")
				return;
		}
		catch { return; }

		Name = x.AttributeN("name").ValueN().NullIfEmptyTrimmed();
		SrcPath = x.AttributeN("src").ValueN().NullIfEmptyTrimmed();
		LogPath = x.AttributeN("log").ValueN().NullIfEmptyTrimmed();
		Pad = x.AttributeN("pad").ValueN().NullIfEmptyTrimmed().ToDoubleN() ?? 0;
	}

	public void Parse(string text)
	{
		Reset();

		string[] lines = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if(lines.IsNulle())
			return;

		string fline = lines[0];
		if(fline[0] == '#' && fline.Contains("<info")) {
			fline = fline[1..];
			SetHeaderValsFromXml(fline);
		}

		for(int i = 0; i < lines.Length; i++) {
			string ln = lines[i];
			if(ln[0] == '#')
				continue;

			var stamp = TrackTimeStamp.ParseCsvString(ln);
			if(stamp == null)
				continue;

			Stamps.Add(stamp);
		}

		Valid = Stamps.Count > 0;
	}

	public void CombineCuts()
	{
		var stamps = Stamps.ToArray();

		if(stamps.IsNulle())
			return;

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

			int countNegs = negIndexSince - i;
			negIndexSince = 0;

			st.CombineFollowingCuts(stamps.AsSpan().Slice(i + 1, countNegs));
		}

		Stamps = stamps.Where(st => !st.IsCut).ToList();
	}
}
