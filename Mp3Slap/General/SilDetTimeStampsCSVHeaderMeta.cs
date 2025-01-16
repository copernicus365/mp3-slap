using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

public record SilDetTimeStampsMeta(
	int count,
	double sdDuration,
	double pad,
	string fileName,
	string filePath,
	FFAudioMeta meta = null)
{
	public SilDetTimeStampsMeta(double pad, string duration, double start = 0, int count = 0, double sdDuration = 0)
		: this(count, sdDuration, pad, null, null, meta: new FFAudioMeta(TimeSpan.Parse(duration), start))
	{ }
}
