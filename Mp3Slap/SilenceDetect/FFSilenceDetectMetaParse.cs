namespace Mp3Slap.SilenceDetect;

public class FFSilenceDetectMetaParse
{
	public bool Success { get; private set; }

	public string Mp3Source { get; private set; }

	public FFAudioMeta Meta { get; private set; }

	public Dictionary<string, string> DictVals { get; private set; }



	public bool Parse(string text)
	{
		Success = false;

		int _startIdx = text.IndexOf("\nInput #");
		if(_startIdx < 5)
			return false;

		string starterTxt = text
			.SubstringMax(_startIdx, 1500)
			.Replace("\r\n", "\n");

		string[] lines = starterTxt
			.Split('\n', count: 20, options: StringSplitOptions.RemoveEmptyEntries);

		if(lines.Length.NotInRange(4, 30))
			return false;

		Mp3Source = _getLn1Src(lines[0]);

		(string ky, string val)[] arr = lines
			.Skip(1)
			.Select(_splitKV)
			.Where(kv => kv.ky != null)
			.ToArray();

		int stopIdx = arr.FindIndex(kv => kv.ky.StartsWith("duration", ignoreCase: true, null));
		if(stopIdx > 0)
			arr = arr.Take(stopIdx + 2).ToArray();

		DictVals = arr.ToDictionaryIgnoreDuplicateKeys(kv => kv.ky, kv => kv.val);

		diceUpDuration();

		DictVals.Add("source", Mp3Source);

		Meta = ToMeta();

		return Success = true;
	}


	public FFAudioMeta ToMeta()
	{
		Dictionary<string, string> d = DictVals;

		FFAudioMeta meta = new(
			source: d.V("source"),
			album: d.V("album"),
			artist: d.V("artist"),
			encoder: d.V("encoder"),
			title: d.V("title"),
			track: d.V("track"),
			TDTG: d.V("tdtg").ToDateTime(),
			genre: d.V("genre"),
			album_artist: d.V("album_artist"),
			date: d.V("date"),
			duration: getTS(d.V("duration")),
			start: d.V("start").ToDouble(),
			bitrate: d.V("bitrate"));

		return meta;
		TimeSpan getTS(string val)
		{
			if(val != null && TimeSpan.TryParse(val, out TimeSpan ts))
				return ts;
			return TimeSpan.Zero;
		}
	}


	void diceUpDuration()
	{
		//   Duration: 04:04:09.81, start: 0.025056, bitrate: 128 kb/s
		// 04:04:09.81, start: 0.025056, bitrate: 128 kb/s

		string durV = DictVals.V("duration");
		if(durV == null)
			return;

		string[] lines = durV
			.Split(',', count: 20, options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if(lines.IsNulle())
			return;

		DictVals["duration"] = lines[0];

		for(int i = 1; i < lines.Length; i++) {
			(string ky, string val) = _splitKV(lines[i]);
			if(ky != null && val != null)
				DictVals[ky] = val;
		}
	}

	static (string ky, string val) _splitKV(string ln)
	{
		ln = ln.NullIfEmptyTrimmed();
		if(ln == null)
			return default;

		int idx = ln.IndexOf(':');
		if(idx < 1)
			return default;

		string ky = ln[0..idx].NullIfEmptyTrimmed();
		string vl = ln[(idx + 1)..].NullIfEmptyTrimmed();

		return (ky?.ToLower(), vl);
	}

	string _getLn1Src(string ln)
	{
		int idx = ln.IndexOf(", from '");
		if(idx < 1)
			return null;
		string lnn = ln[(idx + 8)..^2];
		return lnn;
	}
}
