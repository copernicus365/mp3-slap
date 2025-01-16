namespace Mp3Slap.SilenceDetect;

public record FFAudioMeta(
	string source,
	string album,
	string artist,
	string encoder,
	string title,
	string track,
	DateTime TDTG,
	string genre,
	string album_artist,
	string date,
	TimeSpan duration,
	double start,
	string bitrate)
{
	public FFAudioMeta(TimeSpan duration, double start = 0)
		: this(null, null, null, null, null, null, default, null, null, null, duration, start, null)
	{

	}
}
