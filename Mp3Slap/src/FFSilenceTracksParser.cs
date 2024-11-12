namespace Mp3Slap;

public partial class FFSilenceTracksParser(string text)
{
	readonly string text = text.NullIfEmptyTrimmed();

	public List<TrackTimeStamp> Stamps { get; private set; }

	public List<TrackTimeStamp> Run()
	{
		if(text.IsNullOrEmpty())
			return null;

		Regex removeFrameLines = new("""\nframe=*""");
		string cText = removeFrameLines.Replace(text, v => "");

		Regex rx = RxGetLog();

		double lastEnd = 0;
		int idx = -1;

		Stamps = rx.Matches(cText)
		.Select(m => {
			if(m.Groups.Count < 4)
				return null;

			double start = m.Groups[1].Value.ToDoubleN() ?? -1;
			double end = m.Groups[2].Value.ToDoubleN() ?? -1;
			double sildur = m.Groups[3].Value.ToDoubleN() ?? -1;

			if(start < 0 || end <= start || sildur <= 0)
				return null;

			// difficult because reports at first silence
			// [silencedetect @ 0x600000754240] silence_start: 344.017
			TrackTimeStamp stamp = new(lastEnd, start, sildur);
			lastEnd = end;
			return stamp;
		})
		.Where(v => v != null && v.Duration > TimeSpan.Zero)
		.ToList();

		double bogusDur = 0.001;
		Stamps.Add(new TrackTimeStamp(lastEnd, lastEnd + bogusDur, bogusDur));

		return Stamps;
	}

	//[GeneratedRegex("""\[silencedetect @ 0x\d{4,18}\] silence_start\: (\d+\.\d+)\s?\n\[silencedetect @ 0x\d{4,18}\] silence_end\: (\d+\.\d+) \| silence_duration\: (\d+\.\d+)\n""")]
	[GeneratedRegex("""\[silencedetect @ 0x?[0-9a-zA-Z]{4,18}\] silence_start\: (\d+\.?\d*)\s?\n\[silencedetect @ 0x?[0-9a-zA-Z]{4,18}\] silence_end\: (\d+\.?\d*) \| silence_duration\: (\d+\.?\d*)""")]
	private static partial Regex RxGetLog();
}
