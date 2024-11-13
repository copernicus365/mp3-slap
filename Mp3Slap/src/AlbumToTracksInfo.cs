using System.Text;

namespace Mp3Slap;

/// <summary>
/// AFTER MANY FAILUIRES getting Python script ran by an array to work, I GAVE UP
/// and went back to an extremely simple bash script, also having no echos, just single
/// line calls. ALSO gave up, SADLY!, running a process, just can't get it to work!
/// So generating a script, copy it and run it...
/// </summary>
public class AlbumToTracksInfo(string dir)
{
	public string Dir { get; private set; } = PathHelper.CleanDirPath(dir);

	public bool WriteCsvs { get; set; }

	public string LogDir => $"{Dir}{LogFolderName}/";

	public string[] Paths { get; private set; }

	public string[] RelPaths { get; private set; }

	public string LogFolderName { get; set; } = LogFolderNameDef;

	public bool RemoveRootDirFromScript { get; set; }

	public static string LogFolderNameDef { get; set; } = "logs";

	public void Init(string[] files)
	{
		Paths = files.Select(f => PathHelper.CleanPath(f)).ToArray();
		RelPaths = Paths.Select(f => PathHelper.CleanToRelative(Dir, f)).ToArray();
	}


	public static Mp3ToSplitPathsInfo GetMp3ToSplitPathsInfo(string path, string logDirName)
	{
		path = PathHelper.CleanPath(path);
		string fname = Path.GetFileName(path).NullIfEmptyTrimmed();
		if(fname == null) return default;

		string dir = PathHelper.CleanDirPath(Path.GetDirectoryName(path).NullIfEmptyTrimmed());
		string logDir = $"{dir}{logDirName}/";

		Mp3ToSplitPathsInfo info = new() {
			Directory = dir,
			FileName = fname,
			FilePath = path,
			LogDirectory = logDir,
			SilenceDetectRawLogPath = LogFileNames.GetLogPath(logDir, fname, parsedTxt: false),
			SilenceDetectCsvParsedLogPath = LogFileNames.GetLogPath(logDir, fname, parsedTxt: true)
		};
		// IF not null, above ensures dir trails with '/'
		return info;
	}


	public string Scripts { get; private set; }

	public bool VerboseScript { get; set; }

	public string ScriptsP { get; private set; }

	public List<Mp3ToSplitPathsInfo> Infos;

	public async Task RUN(double silenceInSecs, bool runProcess = true)
	{
		Infos = [];

		string[] paths = PathHelper.GetFilesFromDirectory(Dir, "mp3");

		Init(paths);

		StringBuilder sb = new();
		sb
			.AppendLine("#!/bin/sh")
			.AppendLine()
			.AppendLine("echo \"+++FFMpeg Silence Detect Script +++\"")
			.AppendLine();

		ScriptWriter sw = new();
		Mp3ToSplitPathsInfo first = null;

		for(int i = 0; i < Paths.Length; i++) {
			string inputP = Paths[i];

			Mp3ToSplitPathsInfo info = GetMp3ToSplitPathsInfo(inputP, LogFolderName);

			Infos.Add(info);

			sw.Infos.Add(info);
			if(first == null)
				first = info;

			AlbumLogsWriter awriter = new(info);

			awriter.Init(silenceInSecs, isFirstRun: i == 0);

			string script = $"""
echo --- i: {i,2} run silence detect on: '{info.FileName}' ---

echo log: '{info.SilenceDetectCsvParsedLogPath}

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
					.AppendLine(info.FFMpegScript)
					.AppendLine();
			}

			if(WriteCsvs) {
				awriter.GetAndWriteCsvParsed();
			}
			else if(runProcess) {
				await ProcessHelper.Run("git", script);
			}
		}

		ScriptsP = sw.Write();

		Scripts = sb.ToString();

		Mp3ToSplitPathsInfo fInfo = Infos.FirstOrDefault();

		string scriptPth = $"{fInfo.Directory}run-ffmpeg-silence-det-script-{silenceInSecs}s.sh";

		if(RemoveRootDirFromScript) {
			Scripts = Scripts.Replace(fInfo.Directory, "");
		}

		File.WriteAllText(scriptPth, Scripts);
	}
}

public class ScriptWriter
{
	public List<Mp3ToSplitPathsInfo> Infos = [];

	public string Script { get; set; }

	public string Write()
	{
		var arr = Infos;
		if(arr.IsNulle())
			return Script = null;

		int i = 0;

		string varValsArr = arr.JoinToString(v => $"    ({i++}, \"{v.FileName}\", \"{v.FilePath}\", \"{v.SilenceDetectRawLogPath}\", \"{v.FFMpegScript}\")", ",\n");

		string script = $"""
arr = [
{varValsArr}
]

""";

		string loop1 = """
for i in arr:
    idx = i[0]
    fname = i[1]
    fileP = i[2]
    logP = i[3]
    script = i[4]

    print(f"--- i: {idx} run silence detect on: '{fname}' ---")

    print(f" -log: '{logP}")

    subprocess.call(script, shell=True)

""";

		return Script = script + "\n" + loop1;
	}
}

//public static (string dir, string fname, string value, string logDir) GetSilenceLogPathInfo(
//	string path, bool parsedTxt, string logDirName)
//{
//	path = CleanPath(path);
//	string fname = Path.GetFileName(path).NullIfEmptyTrimmed();
//	if(fname == null) return default;
//	string dir = CleanDirPath(Path.GetDirectoryName(path).NullIfEmptyTrimmed());
//	string logDir = $"{dir}{logDirName}/";
//	// IF not null, above ensures dir trails with '/'
//	string logPath = GetLogPath(logDir, fname, parsedTxt);
//	return (dir, fname, logPath, logDir);
//}

//(string dir, string fname, string value, string logDir)

//public string GetLogPath(string fileName, bool parsedTxt)
//	=> GetLogPath(LogDir, fileName, parsedTxt);

//public static string GetSilenceLogPath(string path, bool parsedTxt, string logFolderName = null)
//	=> GetSilenceLogPathInfo(path, parsedTxt, logFolderName ?? LogFolderNameDef).value;

//public string GetSilenceDetectionCallsToFFMpeg(
//	double silenceInSecondsMin,
//	string inputPath,
//	bool includeFFMpeg = true)
//{
//	string silStr = silenceInSecondsMin.ToString("0.##");

//	var logPathInfo = GetSilenceLogPathInfo(inputPath, parsedTxt: false, LogFolderName);
//	string logPath = logPathInfo.value;
//	string logDir = logPathInfo.logDir;

//	string val = $"""-nostats -i '{inputPath}' -af silencedetect=noise=-30dB:d={silStr} -f null - 2> '{logPath}' """;
//	if(includeFFMpeg)
//		val = "ffmpeg " + val;
//	return val;
//}

//string script = GetSilenceDetectionCallsToFFMpeg(silenceInSecondsMin, inputP, includeFFMpeg: false);
//string logPath = logPathInfo.value;
//SilenceDetLogsWritten.Add(
//	(Paths[i],
//	GetLogPath(logPathInfo.fname, parsedTxt: false),
//	GetLogPath(logPathInfo.fname, parsedTxt: true)));
//var logPathInfo = GetSilenceLogPathInfo(inputP, parsedTxt: true, LogFolderName);
