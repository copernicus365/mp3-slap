using CommandLine.EasyBuilder.Auto;

namespace Mp3Slap.CLI.SilenceDetect;

[Command("silence-detect",
	Alias = "sd",
	Description = "Group for all silence detection related commands")]
public class SilenceDetectGroupCmd { }

[Command("ffmpeg",
	Alias = "ff",
	Description = "Group for converting (single) raw ffmpeg silencedetect output t one per detected input audio file, while converting ff's arcane output to CSV files")]
public class FFMpegGroupCmd { }

[Command("sd-csv",
	Alias = "csv",
	Description = "Group for handling CSV silence detect logs")]
public class SDCsvGroupCmd { }

[Command("group",
	Alias = "gr",
	Description = "Runs subcommands against entire folder of matches for array of input durations")]
public class FullFolderGroupCmd { }
