using System.Text;

using Mp3Slap.Csv;

namespace Mp3Slap.SilenceDetect;

public class MegaSilenceDetectScript
{
	readonly MegaSilenceDetectArgs args;
	readonly double _silenceDuration;

	public string Dir => args.Directory;
	public bool RemoveRootDirFromScript => args.WriteRelativePaths;
	public bool VerboseScript => args.Verbose;

	public string LogFolderName { get; private set; }


	public MegaSilenceDetectScript(MegaSilenceDetectArgs args, double silenceDuration)
	{
		this.args = args;
		_silenceDuration = silenceDuration;
		LogFolderName = args.GetLogFolderName(_silenceDuration, fullPath: false);
	}

	public bool WriteCsvs { get; set; }

	public string[] Paths { get; private set; }

	public string[] RelPaths { get; private set; }


	public void Init(string[] files)
	{
		Paths = files.Select(f => PathHelper.CleanPath(f)).ToArray();
		RelPaths = Paths.Select(f => PathHelper.CleanToRelative(Dir, f)).ToArray();
	}

	public static Mp3ToSplitPathsInfo GetMp3ToSplitPathsInfo(string path, string logDirName, double silenceDur)
	{
		path = PathHelper.CleanPath(path);
		string fname = Path.GetFileName(path).NullIfEmptyTrimmed();
		if(fname == null) return default;

		string dir = PathHelper.CleanDirPath(Path.GetDirectoryName(path).NullIfEmptyTrimmed());
		string logDir = $"{dir}{logDirName}";

		Mp3ToSplitPathsInfo info = new() {
			Directory = dir,
			FileName = fname,
			FilePath = path,
			LogDirectory = logDir,
			SilenceDetectRawLogPath = LogFileNames.GetLogPath(logDir, fname, parsedTxt: false),
			SilenceDetectCsvParsedLogPath = LogFileNames.GetLogPath(logDir, fname, parsedTxt: true),
			AuditionMarkersCsvPath = LogFileNames.GetAuditionMarkersPath(logDir, fname),
			SilenceDuration = silenceDur,
		};
		info.Init();
		return info;
	}


	public string Scripts { get; private set; }


	public List<Mp3ToSplitPathsInfo> Infos;



	StringBuilder sb;

	public void SetInfos()
	{
		Infos = [];

		string[] paths = PathHelper.GetFilesFromDirectory(
			Dir,
			searchPattern: args.AudioFilesSearchPattern,
			includeSubDirectories: args.IncludeSubDirectories);

		if(paths.IsNulle()) {
			$"No files found".Print();
			return;
		}

		Init(paths);

		for(int i = 0; i < Paths.Length; i++) {
			string audioFullP = Paths[i];

			Mp3ToSplitPathsInfo info = GetMp3ToSplitPathsInfo(audioFullP, LogFolderName, _silenceDuration);
			Infos.Add(info);

			if(i == 0 && !Directory.Exists(info.LogDirectory))
				Directory.CreateDirectory(info.LogDirectory);
		}
	}

	public async Task RUN_Write_AuditionMarkerCSVs_From_SilenceCSVs()
	{
		for(int i = 0; i < Infos.Count; i++) {
			Mp3ToSplitPathsInfo info = Infos[i];

			if(!File.Exists(info.SilenceDetectCsvParsedLogPath))
				continue;

			string silenceDetCsvLog = File.ReadAllText(info.SilenceDetectCsvParsedLogPath);

			TrackTimeStampsCsvV2 csv2 = new();
			info.Stamps = csv2.Parse(silenceDetCsvLog);

			AuditionCsv acsv = new();
			acsv.SetMarkers(info.Stamps, "Ch ", firstDesc: $"From mp3-slap silence csv logs - {csv2.FileName}");

			string audCsv = acsv.WriteCsv(includeHeader: true);

			File.WriteAllText(info.AuditionMarkersCsvPath, audCsv);
		}
	}

	public async Task RUN_All()
	{
		if(Infos.IsNulle())
			return;

		_startScript();

		for(int i = 0; i < Infos.Count; i++) {
			Mp3ToSplitPathsInfo info = Infos[i];

			_addScipt(i, info);

			if(!args.RunFFScript)
				continue;

			info.FFSplitOutput = await ProcessHelper.RunFFMpegProcess(info.FFMpegScriptArgsNoLogPath);

			if(args.WriteFFMpegSilenceLogs)
				File.WriteAllText(info.SilenceDetectRawLogPath, info.FFSplitOutput);

			ParseStampsAndWrite(info);
		}

		Scripts = sb.ToString();

		_writeFinalCombinedFFMpegShellScriptsToFile(Infos.First().Directory);
	}

	public void ParseStampsAndWrite(Mp3ToSplitPathsInfo info)
	{
		FFSilenceTracksParser split = new(
			text: info.FFSplitOutput,
			pad: args.Pad);

		info.Stamps = split.Run();
		//split.Stamps.SetPads(args.Pad);

		TrackTimeStampsCsvV2 csv2 = new();
		csv2.InitForWrite(info.FileName, args.Pad, split.Stamps, filePath: info.FilePath);

		string csvContent = csv2.Write();

		File.WriteAllText(info.SilenceDetectCsvParsedLogPath, csvContent);
	}

	void _startScript()
	{
		sb = new();
		sb
			.AppendLine("#!/bin/sh")
			.AppendLine()
			.AppendLine("echo \"+++FFMpeg Silence Detect Script +++\"")
			.AppendLine();
	}

	void _addScipt(
		int i,
		Mp3ToSplitPathsInfo info)
	{
		string script = $"""
echo "--- i: {i,2} run silence detect on: '{info.FileName}' ---"

echo "log: '{info.SilenceDetectCsvParsedLogPath}"

sleep 2

{info.FFMpegScript}
""";
		script = script.Trim('"'); // ?! can't get """ type string to not put "quotes"!

		if(VerboseScript) {
			sb
				.AppendLine(script)
				.AppendLine();
		}
		else {
			sb
				.AppendLine(info.GetFFMpegScript(withLogPaths: true))
				.AppendLine();
		}
	}

	void _writeFinalCombinedFFMpegShellScriptsToFile(string directory)
	{
		string scriptPth = $"{directory}run-ffmpeg-silence-det-script-{_silenceDuration}s.sh";

		if(RemoveRootDirFromScript) {
			Scripts = Scripts.Replace(directory, "");
		}

		if(args.WriteFFMpegSilenceLogs)
			File.WriteAllText(scriptPth, Scripts);
	}


	public static async Task<MegaSilenceDetectScript[]> RunManyDurations(MegaSilenceDetectArgs args, Func<MegaSilenceDetectScript, Task> run)
	{
		double[] durations = args.SilenceDurations;

		MegaSilenceDetectScript[] scripts = new MegaSilenceDetectScript[durations.Length];

		for(int i = 0; i < durations.Length; i++) {
			double silenceDur = durations[i];

			MegaSilenceDetectScript script = scripts[i] = new(args, silenceDur);

			script.SetInfos();

			await run(script);

			string scr = script.Scripts;
		}
		return scripts;
	}

}
