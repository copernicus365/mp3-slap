using System.Text;

namespace Mp3Slap;

public class AlbumToTracksInfo
{
	public string Dir { get; private set; }

	public string LogDir { get; private set; }

	public string[] Paths { get; private set; }

	public string[] RelPaths { get; private set; }

	public AlbumToTracksInfo(string dir)
	{
		Dir = CleanDirPath(dir);
		LogDir = $"{Dir}logs/";
	}

	public void Init(string[] files)
	{
		Paths = files.Select(f => CleanPath(f)).ToArray();
		RelPaths = Paths.Select(f => CleanToRelative(Dir, f)).ToArray();
	}


	public static (string dir, string fname, string value, string logDir) GetSilenceLogPathInfo(
		string path, bool parsedTxt)
	{
		path = CleanPath(path);
		string fname = Path.GetFileName(path).NullIfEmptyTrimmed();
		if(fname == null) return default;
		string dir = CleanDirPath(Path.GetDirectoryName(path).NullIfEmptyTrimmed());
		string logDir = $"{dir}logs/";
		// IF not null, above ensures dir trails with '/'
		string logPath = GetLogPath(logDir, fname, parsedTxt);
		return (dir, fname, logPath, logDir);
	}

	public static string GetLogPath(string logDir, string fileName, bool parsedTxt)
	{
		string logPath = $"{logDir}{fileName}-silence-detect" + (!parsedTxt
			? ".log"
			: "-parsed.txt");
		return logPath;
	}

	public string GetLogPath(string fileName, bool parsedTxt)
		=> GetLogPath(LogDir, fileName, parsedTxt);

	public static string GetSilenceLogPath(string path, bool parsedTxt)
		=> GetSilenceLogPathInfo(path, parsedTxt).value;

	public static string[] GetFilesFromDirectory(
		string dir,
		string extensionType = "mp3",
		bool includeSubDirectories = false)
	{
		extensionType = extensionType.NullIfEmptyTrimmed();
		ArgumentNullException.ThrowIfNullOrEmpty(extensionType);

		dir = CleanDirPath(dir);

		if(!Directory.Exists(dir))
			throw new DirectoryNotFoundException(dir);

		string[] files = Directory.GetFiles(dir, "*." + extensionType, includeSubDirectories
			? SearchOption.AllDirectories
			: SearchOption.TopDirectoryOnly);

		return files;
	}

	public static string CleanDirPath(string dir)
	{
		dir = dir.NullIfEmptyTrimmed();
		if(dir == null) return null;
		dir = CleanPath(dir);
		if(dir.Last() != '/')
			dir += '/';
		return dir;
	}

	public static string CleanPath(string path) => path.Replace('\\', '/');

	public static string CleanToRelative(string dir, string path)
		=> path.StartsWith(dir) ? path[dir.Length..] : path;

	public string GetSilenceDetectionCallsToFFMpeg(
		double silenceInSecondsMin,
		string inputPath,
		bool includeFFMpeg = true)
	{
		string silStr = silenceInSecondsMin.ToString("0.##");

		var logPathInfo = GetSilenceLogPathInfo(inputPath, parsedTxt: true);
		string logPath = logPathInfo.value;
		string logDir = logPathInfo.logDir;

		string val = $"""-nostats -i '{inputPath}' -af silencedetect=noise=-30dB:d={silStr} -f null - 2> '{logPath}' """;
		if(includeFFMpeg)
			val = "ffmpeg " + val;
		return val;
	}

	public string SilenceDetectScripts { get; private set; }

	public List<(string inputFile, string logPath, string logPathParsed)> SilenceDetLogsWritten;

	public async Task RunSilenceDetectScript_1(
		double silenceInSecondsMin,
		bool runProcess = true)
	{
		SilenceDetLogsWritten = [];

		string[] paths = GetFilesFromDirectory(Dir, "mp3");

		Init(paths);

		StringBuilder sb = new();

		for(int i = 0; i < Paths.Length; i++) {
			string inputP = Paths[i];

			string script = GetSilenceDetectionCallsToFFMpeg(silenceInSecondsMin, inputP, includeFFMpeg: false);

			var logPathInfo = GetSilenceLogPathInfo(inputP, parsedTxt: true);

			string logPath = logPathInfo.value;

			if(i == 0 && !Directory.Exists(LogDir)) {
				Directory.CreateDirectory(LogDir);
			}

			SilenceDetLogsWritten.Add(
				(Paths[i],
				GetLogPath(logPathInfo.fname, parsedTxt: false),
				GetLogPath(logPathInfo.fname, parsedTxt: true)));

			sb.AppendLine($"# write silence detect, to log at '{logPath}'\n");

			sb.AppendLine("ffmpeg " + script);

			if(runProcess) {
				await ProcessHelper.Run("ffmpeg", script);
			}
		}

		SilenceDetectScripts = sb.ToString();
	}
}
