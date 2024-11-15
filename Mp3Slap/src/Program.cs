using static System.Console;

namespace Mp3Slap;

public class Runner
{
	public static async Task Main(string[] args)
		=> await new Runner().RUN(args);

	static string TestPath = "C:/Dropbox/Music/Bible/Suchet-NIV-1Album";
	// "C:/Dropbox/Vids/mp3-split/mp3-split/test1"

	public async Task RUN(string[] args)
	{
		if(args != null)
			args = args.Select(v => v.NullIfEmptyTrimmed()).Where(v => v != null).ToArray();

		WriteLine("mp3 slap!");

		bool elseUseDef = true;
		if(elseUseDef && args.IsNulle()) {
			args = ["C:/Dropbox/Music/Bible/Suchet-NIV-1Album"];
		}

		RunnerSettings stg = null;

		GetSettingsPath getStgsP = new();
		if(!getStgsP.RUN(args?.FirstOrDefault())) {
			WriteLine(getStgsP.Error);
			return;
		}

		if(stg == null) {
			stg = RunnerSettings.TryParse(getStgsP.SettingsContent, getStgsP.RootDirectory);
			if(stg == null) {
				WriteLine("Invalid settings");
				return;
			}
		}

		double[] silenceLens = stg.SilenceLengths; // [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];

		for(int i = 0; i < silenceLens.Length; i++) {
			double silenceLen = silenceLens[i];

			string logsNm = $"logs1-{silenceLen}s";

			switch(stg.Type) {
				case RunnerType.WriteFFMpegSilenceScript: {
					AlbumToTracksInfo ati = new(stg.Directory) {
						LogFolderName = logsNm,
						RemoveRootDirFromScript = false,
					};

					await ati.RUN(silenceInSecs: silenceLen, runProcess: false);

					string script = ati.Scripts;

					break;
				}
				case RunnerType.ConvertFFMpegSilenceLogsToCSVs: {
					// write csvs from raw results
					RunParserOnLogsFolder ff = new();
					ff.RUN(logsDir: stg.Directory + '/' + logsNm);
					break;
				}
				//case RunnerType.WriteSettings: {
				//	RunnerSettings stgs = new() {

				//	};
				//}
			}
		}
		WriteLine("!end!");
	}
}


public enum RunnerType
{
	None = 0,
	WriteFFMpegSilenceScript = 1,
	ConvertFFMpegSilenceLogsToCSVs = 2,
	//WriteSettings = 8,
}

public class RunnerSettings
{
	public RunnerType Type { get; set; }

	public double[] SilenceLengths { get; set; }

	public string Directory { get; set; }


	public static RunnerSettings TryParse(string content, string directory)
	{
		if(content.IsNulle() || !content.Contains('{'))
			return null;

		try {
			RunnerSettings stg = content.DeserializeJson<RunnerSettings>();
			if(stg.Type == RunnerType.None)
				return null; // just taking this as sign contents wasn't close to valid

			if(stg.Directory == null)
				stg.Directory = directory;
			return stg;
		}
		catch {
			return null;
		}
	}

}
