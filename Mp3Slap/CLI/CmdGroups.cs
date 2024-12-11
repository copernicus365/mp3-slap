using CommandLine.EasyBuilder.Auto;

namespace Mp3Slap.CLI.SilenceDetect;

[Command("silence-detect",
	Alias = "sd",
	Description = "Grouping of silence detection related commands")]
public class SilenceDetectGroupCmd { }

[Command("ffmpeg",
	Alias = "ff",
	Description = "Runs ffmpeg silencedetect scripts, or at least generates scripts that call ffmpeg to do this, one per detected input audio file, while converting ff's arcane output to CSV files")]
public class FFMpegGroupCmd { }
