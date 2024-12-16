namespace Mp3Slap.SilenceDetect;

public partial class FFSilenceTracksParser(string text, double pad = FFSilenceTracksParser.PadDefault)
{
	readonly string text = text.NullIfEmptyTrimmed();

	public const double PadDefault = 0.3;

	public FFAudioMeta Meta { get; private set; }

	public List<TrackTimeStamp> Stamps { get; private set; }

	public void SetMeta()
	{
		FFSilenceDetectMetaParse mp = new();
		mp.Parse(text);
		Meta = mp.Meta;
	}

	public List<TrackTimeStamp> Parse(bool setMeta = true)
	{
		if(text.IsNullOrEmpty())
			return null;

		if(setMeta)
			SetMeta();

		Regex removeFrameLines = new("""\nframe=*""");
		string cText = removeFrameLines.Replace(text, v => "");

		Regex rx = RxGetLog();

		double lastEnd = 0;

		Match[] matches = rx.Matches(cText).ToArray();

		Stamps = new(matches.Length + 1);

		for(int i = 0; i < matches.Length; i++) {
			Match m = matches[i];

			if(m.Groups.Count < 4)
				return null;

			double start = m.Groups[1].Value.ToDoubleN() ?? -1;
			double end = m.Groups[2].Value.ToDoubleN() ?? -1;
			double silenceDur = m.Groups[3].Value.ToDoubleN() ?? -1;

			if(start < 0 || end <= start || silenceDur <= 0)
				return null;

			// difficult because reports at first silence
			// [silencedetect @ 0x600000754240] silence_start: 344.017
			TrackTimeStamp stamp = new(lastEnd, start, silenceDur, pad: pad);
			lastEnd = end;

			if(stamp == null || stamp.Duration <= TimeSpan.Zero)
				continue;

			Stamps.Add(stamp);
		}

		if(Meta != null && Meta.duration > TimeSpan.Zero) {
			double endDur = Meta.duration.TotalSeconds;

			double sil = endDur - lastEnd;
			Stamps.Add(new TrackTimeStamp(lastEnd, endDur, 0, pad: pad));
		}
		else {
			double endDur = 0.001;
			Stamps.Add(new TrackTimeStamp(lastEnd, lastEnd + endDur, endDur, pad: pad));
		}

		return Stamps;
	}

	//[GeneratedRegex("""\[silencedetect @ 0x\d{4,18}\] silence_start\: (\d+\.\d+)\s?\n\[silencedetect @ 0x\d{4,18}\] silence_end\: (\d+\.\d+) \| silence_duration\: (\d+\.\d+)\n""")]
	[GeneratedRegex("""\[silencedetect @ 0x?[0-9a-zA-Z]{4,18}\] silence_start\: (\d+\.?\d*)\s?\n\[silencedetect @ 0x?[0-9a-zA-Z]{4,18}\] silence_end\: (\d+\.?\d*) \| silence_duration\: (\d+\.?\d*)""")]
	private static partial Regex RxGetLog();
}
