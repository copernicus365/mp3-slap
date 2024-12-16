using System.Text;
using System.Xml.Linq;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

public class TrackTimeStampsCsv
{
	public string FileName { get; set; }

	public int Count => Stamps?.Count ?? 0;

	public double Pad { get; set; }

	public string FilePath { get; set; }

	public List<TrackTimeStamp> Stamps { get; set; }

	public bool Valid { get; set; }

	public void InitForWrite(
		string fileName,
		double pad,
		List<TrackTimeStamp> stamps,
		FFAudioMeta meta = null,
		string filePath = null)
	{
		Pad = pad;
		Stamps = stamps;

		if(fileName.IsNullOrHasNone() && filePath.NotNulle()) {
			FileName = Path.GetFileName(filePath);
			FilePath = filePath; // Path.GetFullPath(filePath);
		}
		else {
			FileName = fileName ?? "";
			FilePath = filePath ?? Path.GetFileName(FileName);
		}
	}

	public string Write()
	{
		StringBuilder sb = new();

		string x = GetHeaderXml();
		sb.AppendLine($"# {x}");

		for(int i = 0; i < Stamps.Count; i++) {
			TrackTimeStamp st = Stamps[i];
			string res = st.ToCsvString2();
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
			new XAttribute("fileName", FileName ?? ""),
			new XAttribute("count", Count),
			new XAttribute("pad", Pad),
			new XAttribute("filePath", FilePath ?? ""))
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

		FileName = x.AttributeN("fileName").ValueN().NullIfEmptyTrimmed();
		FilePath = x.AttributeN("filePath").ValueN().NullIfEmptyTrimmed();
		Pad = x.AttributeN("pad").ValueN().NullIfEmptyTrimmed().ToDoubleN() ?? 0;
	}

	public List<TrackTimeStamp> Parse(string text)
	{
		string[] lines = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if(lines.IsNulle())
			return null;

		string fline = lines[0];
		if(fline[0] == '#' && fline.Contains("<info")) {
			fline = fline[1..];
			SetHeaderValsFromXml(fline);
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

		Valid = Stamps.Count > 0;
		return Stamps;
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
