namespace Mp3Slap.SilenceDetect;

/// <summary>
/// Silence detect script stuff for an entire folder, but only for ONE duration.
/// </summary>
public class SilenceDetectFullFolderScript(SilenceDetectFullFolderArgs args, double silenceDuration)
{
	public string LogFolderName { get; } = args.GetLogFolderName(silenceDuration, fullPath: false);

	public bool Verbose => args.Verbose;

	// ---

	public string[] Paths { get; private set; }

	public string[] RelPaths { get; private set; }

	// ---

	public List<Mp3ToSplitPathsInfo> Infos;


	/// <summary>
	/// Inits <see cref="Infos"/> and other path related stuff. RUNNING THIS
	/// sets the key stuff that makes this type, meaning <see cref="RUN_ALL"/>
	/// do not have to be run for this to be, critically, used by other types. 
	/// </summary>
	public void INIT()
	{
		Infos = [];

		string[] audioFilePaths = PathHelper.GetFilesFromDirectory(args);

		if(audioFilePaths.IsNulle()) {
			$"No files found".Print();
			return;
		}

		Paths = audioFilePaths.Select(f => PathHelper.CleanPath(f)).ToArray();
		RelPaths = Paths.Select(f => PathHelper.CleanToRelative(args.Directory, f)).ToArray();

		for(int i = 0; i < Paths.Length; i++) {
			string audioFullP = Paths[i];

			Mp3ToSplitPathsInfo info = GetMp3ToSplitPathsInfo(audioFullP, LogFolderName, silenceDuration);
			Infos.Add(info);

			if(i == 0 && !Directory.Exists(info.LogDirectory))
				Directory.CreateDirectory(info.LogDirectory);
		}
	}

	/// <summary>
	/// Called by <see cref="INIT"/> above, but as static might be used elsewhere
	/// </summary>
	public static Mp3ToSplitPathsInfo GetMp3ToSplitPathsInfo(string path, string logDirName, double silenceDur)
	{
		path = PathHelper.CleanPath(path);
		string fname = Path.GetFileName(path).NullIfEmptyTrimmed();
		if(fname == null) return default;

		string dir = PathHelper.CleanDirPath(Path.GetDirectoryName(path).NullIfEmptyTrimmed());
		string logDir = $"{dir}{logDirName}";

		Mp3ToSplitPathsInfo info = new() {
			Directory = dir,
			AudioFileName = fname,
			AudioFilePath = path,
			LogDirectory = logDir,
			SilenceDuration = silenceDur,
		};
		info.SetLogPaths();
		return info;
	}


	public async Task RUN_ALL()
	{
		if(Infos.IsNulle())
			return;

		SilDetectWriteFFScript writeScript = new();
		writeScript.StartScript();

		for(int i = 0; i < Infos.Count; i++) {
			Mp3ToSplitPathsInfo info = Infos[i];
			SilenceDetectRunSingle single = new(info, args, writeScript, i);
			await single.RUN();
		}

		Mp3ToSplitPathsInfo inf = Infos.First();

		string scriptPth = $"{inf.LogDirectory}run-ffmpeg-silence-det-script.sh"; //-{_silenceDuration}s.sh";

		writeScript.WriteFinalCombinedFFMpegShellScriptsToFile(inf, scriptPth, args.WriteRelativePaths, args.WriteFFMpegSilenceLogs);
	}

	public static async Task<SilenceDetectFullFolderScript[]> RunManyDurations(
		SilenceDetectFullFolderArgs args,
		Func<SilenceDetectFullFolderScript, Task> run)
	{
		double[] durations = args.SilenceDurations;

		SilenceDetectFullFolderScript[] scripts = new SilenceDetectFullFolderScript[durations.Length];

		for(int i = 0; i < durations.Length; i++) {
			double silenceDur = durations[i];

			SilenceDetectFullFolderScript script = scripts[i] = new(args, silenceDur);

			script.INIT();

			await run(script);
		}
		return scripts;
	}
}
