using System.Text;

namespace Mp3Slap.SilenceDetect;

/// <summary>
/// AFTER MANY FAILUIRES getting Python script ran by an array to work, I GAVE UP
/// and went back to an extremely simple bash script, also having no echos, just single
/// line calls. ALSO gave up, SADLY!, running a process, just can't get it to work!
/// So generating a script, copy it and run it...
/// </summary>
public class WriteFFMpegSilenceDetectScript
{
	readonly SilenceDetectWriteFFMpegScriptArgs args;
	readonly double _silenceDuration;
	
	public string Dir => args.Directory;
	public bool RemoveRootDirFromScript => args.WriteRelativePaths;
	public bool VerboseScript => args.Verbose;

	public string LogFolderName { get; private set; }


	public WriteFFMpegSilenceDetectScript(SilenceDetectWriteFFMpegScriptArgs args, double silenceDuration)
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

	public string ScriptsP { get; private set; }

	public List<Mp3ToSplitPathsInfo> Infos;

	public async Task RUN()
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

			awriter.Init(_silenceDuration, isFirstRun: i == 0);

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
				$"".Print();
				awriter.GetAndWriteCsvParsed();
			}
			//else if(runProcess) {
			//	await ProcessHelper.Run("git", script);
			//}
		}

		ScriptsP = sw.Write();

		Scripts = sb.ToString();

		Mp3ToSplitPathsInfo fInfo = Infos.FirstOrDefault();

		string scriptPth = $"{fInfo.Directory}run-ffmpeg-silence-det-script-{_silenceDuration}s.sh";

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
