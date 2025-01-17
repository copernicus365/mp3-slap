using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

public record SilDetTimeStampsMeta(
	int count = 0,
	double sdDuration = 0,
	double pad = 0,
	string fileName = null,
	string filePath = null,
	FFAudioMeta meta = null)
{
	public SilDetTimeStampsMeta() : this(0, 0, 0, null, null, null) { }

	public SilDetTimeStampsMeta(double pad, string duration, double start = 0, int count = 0, double sdDuration = 0)
		: this(count, sdDuration, pad, null, null, meta: new FFAudioMeta(TimeSpan.Parse(duration), start))
	{ }
}
