using System.Text;

using DotNetXtensions;

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
			SilenceDuration = silenceDur,
		};
		info.Init();
		return info;
	}


	public string Scripts { get; private set; }


	public List<Mp3ToSplitPathsInfo> Infos;



	StringBuilder sb;

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

		_startScript();

		for(int i = 0; i < Paths.Length; i++) {
			string inputP = Paths[i];

			Mp3ToSplitPathsInfo info = GetMp3ToSplitPathsInfo(inputP, LogFolderName, _silenceDuration);
			Infos.Add(info);

			if(i == 0 && !Directory.Exists(info.LogDirectory))
				Directory.CreateDirectory(info.LogDirectory);

			_addScipt(i, info);

			if(args.RunFFScript) {
				info.FFSplitOutput = await ProcessHelper.RunFFMpegProcess(info.FFMpegScriptArgsNoLogPath);		
			}
		}

		Scripts = sb.ToString();

		_writeFinalCombinedFFMpegShellScriptsToFile(Infos.First().Directory);
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
}
