using Mp3Slap.SilenceDetect;

namespace Mp3Slap.General;

public static class PathHelper
{
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

	public static string[] GetFilesFromDirectory(SilenceDetectFullFolderArgs args)
	{
		string[] paths = GetFilesFromDirectory(
			args.Directory,
			searchPattern: args.AudioFilesSearchPattern,
			includeSubDirectories: args.IncludeSubDirectories);
		return paths;
	}

	public static string[] GetFilesFromDirectory(
		string dir,
		string searchPattern,
		bool includeSubDirectories = false)
	{
		searchPattern = searchPattern.NullIfEmptyTrimmed();
		ArgumentException.ThrowIfNullOrEmpty(searchPattern);

		dir = CleanDirPath(dir);

		if(!Directory.Exists(dir))
			throw new DirectoryNotFoundException(dir);

		string[] files = Directory.GetFiles(
			path: dir,
			searchPattern: searchPattern,
			searchOption: includeSubDirectories
			? SearchOption.AllDirectories
			: SearchOption.TopDirectoryOnly);

		return files;
	}

	public static string[] GetSilenceDetectLogPaths(string logsDir, bool includeSubDirs = false)
	{
		string[] logsPaths = GetFilesFromDirectory(logsDir, "*silencedetect.log", includeSubDirectories: includeSubDirs);
		return logsPaths;
	}

	public static string[] GetSilenceDetectCsvLogPaths(string logsDir, bool includeSubDirs = false)
	{
		string[] logsPaths = GetFilesFromDirectory(logsDir, "*silencedetect-parsed.csv", includeSubDirectories: includeSubDirs);
		return logsPaths;
	}
}

public static class GenHelperX
{
	public static string DigitsCountFrmt(this int len)
	{
		if(len < 10)
			return "0";
		if(len < 100)
			return "00";
		if(len >= 100)
			return "000";
		if(len >= 1000)
			return "0000";
		return "00000";
	}
}
