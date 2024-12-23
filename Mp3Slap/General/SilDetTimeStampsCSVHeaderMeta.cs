using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

public record SilDetTimeStampsMeta(
	int count,
	double duration,
	double pad,
	string fileName,
	string filePath,
	FFAudioMeta meta = null);