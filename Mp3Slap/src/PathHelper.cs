namespace Mp3Slap;

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

	public static string[] GetFilesFromDirectory(
		string dir,
		string extensionType = "mp3",
		bool includeSubDirectories = false)
	{
		extensionType = extensionType.NullIfEmptyTrimmed();
		ArgumentException.ThrowIfNullOrEmpty(extensionType);

		dir = CleanDirPath(dir);

		if(!Directory.Exists(dir))
			throw new DirectoryNotFoundException(dir);

		string[] files = Directory.GetFiles(dir, "*." + extensionType, includeSubDirectories
			? SearchOption.AllDirectories
			: SearchOption.TopDirectoryOnly);

		return files;
	}

}
