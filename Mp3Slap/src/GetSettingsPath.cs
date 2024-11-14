namespace Mp3Slap;

public class GetSettingsPath
{
	public const string StandardJsonMp3SlapName = "mp3slap";

	public string RootDirectory { get; set; }

	public string SettingsPath { get; set; }

	public string Error { get; set; }

	public bool Success { get; set; }

	public string SettingsContent { get; set; }

	bool setErr(string msg)
	{
		Error = msg;
		return Success = false;
	}

	public bool RUN(string arg)
	{
		if(!TryGetSettingsPath(arg))
			return false;

		SettingsContent = File.ReadAllText(SettingsPath);
		if(SettingsContent == null)
			return setErr("No content");

		return true;
	}

	public bool TryGetSettingsPath(string arg)
	{
		string dir = TryParseArgPath(arg, out string filePath);
		if(dir == null)
			dir = GetEnvDir();

		if(dir == null)
			return setErr("No directory");

		if(!Directory.Exists(dir))
			return setErr($"Invalid arg: directory '{dir}' doesn't exist");

		RootDirectory = dir;

		if(filePath != null) {
			if(!File.Exists(filePath))
				return setErr($"Invalid arg: file '{filePath}' doesn't exist");
		}

		if(filePath == null) {
			string[] files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly)
				.Select(v => PathHelper.CleanPath(v))
				.ToArray();

			if(files.IsNulle())
				return setErr($"Invalid directory: no settings .json files found");

			if(files.Length == 1)
				filePath = files[0];
			else {
				string exp = '/' + StandardJsonMp3SlapName + ".json";

				filePath = files.FirstOrDefault(v => v.EndsWith(exp));

				if(filePath == null) {
					return setErr($"Json files existed but none with name '{exp}'");
				}
			}
		}

		SettingsPath = filePath;
		return true;
	}


	public static string TryParseArgPath(string arg, out string filePath)
	{
		filePath = null;
		arg = arg.NullIfEmptyTrimmed();
		string dir = null;

		if(arg == null)
			return null;

		if(arg.EndsWith(".json")) {
			dir = Path.GetDirectoryName(arg);
			filePath = PathHelper.CleanPath(arg);
		}
		else
			dir = arg;

		dir = PathHelper.CleanDirPath(dir);

		return dir;
	}

	public static string GetEnvDir()
	{
		string dir = Environment.CurrentDirectory.NullIfEmptyTrimmed();
		if(dir == null)
			return null;

		dir = PathHelper.CleanDirPath(dir);
		return dir;
	}

}
