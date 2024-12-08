using System.Text;

namespace Mp3Slap.General;

public class AuditionCsv
{
	public const string HeaderDef = "Name\tStart\tDuration\tTime Format\tType\tDescription";

	public List<AuditionMarker> Markers { get; set; } = [];

	public void SetMarkers(IList<TrackTimeStamp> stamps, string namePrefix, string firstDesc = null)
	{
		if(stamps.IsNulle())
			return;

		string nfrmt = stamps.Count.DigitsCountFrmt();

		Markers = stamps
			.Select((x, i) => AuditionMarker.ToMarker(x, $"{namePrefix}{(i + 1).ToString(nfrmt)}"))
			.ToList();

		if(firstDesc != null)
			Markers.First().Description = firstDesc;
	}

	public string WriteCsv(
		bool includeHeader = true)
	{
		if(Markers.IsNulle())
			return null;

		StringBuilder sb = new((Markers.Count + 1) * 50);

		if(includeHeader)
			sb.AppendLine(HeaderDef);

		foreach(AuditionMarker marker in Markers)
			sb.AppendLine(marker.ToCsv());

		string result = sb.ToString();
		return result;
	}
}

public class AuditionMarker
{
	public AuditionMarker() { }

	public AuditionMarker(string name, TimeSpan start, TimeSpan duration, string description = null)
	{
		Name = name;
		Start = start;
		Duration = duration;
		Description = description;
		Type = AuditionMarkerType.Cue;
	}

	public string Name { get; set; }

	public TimeSpan Start { get; set; }

	public TimeSpan Duration { get; set; }

	public string TimeFormat => "decimal";

	public AuditionMarkerType Type { get; set; }

	public string Description { get; set; }

	public string ToCsv()
		=> $"{CsvFuncs.EscapeCsv(Name)}\t{Start.ToString(TSFrmt)}\t{Duration.ToString(TSFrmt)}\t{TimeFormat}\t{Type}\t{CsvFuncs.EscapeCsv(Description)}";

	public override string ToString() => ToCsv();

	public const string TSFrmt = @"h\:mm\:ss\.fff";

	public static AuditionMarker ToMarker(TrackTimeStamp stamp, string name, string desc = null)
		=> new(name, stamp.PaddedStart, stamp.PaddedDuration, desc);
}

public enum AuditionMarkerType : byte
{
	Cue = 0,
	Subclip = 1,
	CDTrack = 2,
	CartTimer = 3,
}

/*
Name	Start	Duration	Time Format	Type	Description
Marker 02	9:39.590	0:00.000	decimal	Cue	
Marker 01	1:44:53.111	0:00.000	decimal	Cue
*/
