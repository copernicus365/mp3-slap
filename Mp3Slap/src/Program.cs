using static System.Console;

namespace Mp3Slap;

public class Runner
{
	public static async Task Main1(string[] args)
		=> await new Runner().RUN(args);

	static string TestPath = "C:/Dropbox/Music/Bible/Suchet-NIV-1Album";
	// "C:/Dropbox/Vids/mp3-split/mp3-split/test1"

	public async Task RUN(string[] args)
	{
		if(args != null)
			args = args.Select(v => v.NullIfEmptyTrimmed()).Where(v => v != null).ToArray();

		string comLine = PathHelper.CleanPath(Environment.CommandLine);

		WriteLine("mp3 slap!");

		bool elseUseDef = true;
		if(elseUseDef && args.IsNulle()) {
			args = ["C:/Dropbox/Music/Bible/Suchet-NIV-1Album"];
		}

		SilenceDetectRunSettingsOld stg = null;

		GetSettingsPath getStgsP = new();
		if(!getStgsP.RUN(args?.FirstOrDefault())) {
			WriteLine(getStgsP.Error);
			return;
		}

		if(stg == null) {
			stg = SilenceDetectRunSettingsOld.TryParse(getStgsP.SettingsContent, getStgsP.RootDirectory);
			if(stg == null) {
				WriteLine("Invalid settings");
				return;
			}
		}

		double[] silenceLens = stg.SilenceLengths; // [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];

		// double[] silenceLens / [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7]


		for(int i = 0; i < silenceLens.Length; i++) {
			double silenceLen = silenceLens[i];

			string logsNm = $"logs1-{silenceLen}s";

			switch(stg.Type) {
				// silencedetect write-ffmpeg-script
				case RunnerType.WriteFFMpegSilenceScript: { //silence
					AlbumToTracksInfo ati = new(stg.Directory) {
						LogFolderName = logsNm,
						RemoveRootDirFromScript = false,
					};

					await ati.RUN(silenceInSecs: silenceLen, runProcess: false);

					string script = ati.Scripts;

					break;
				}
				// silencedetect tocsv
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

	public async Task RUN2(SilenceDetectRunSettings stg)
	{
		double[] silenceLens = stg.SilenceLengths; // [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];

		// double[] silenceLens / [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7]

		for(int i = 0; i < silenceLens.Length; i++) {
			double silenceLen = silenceLens[i];

			string logsNm = $"logs1-{silenceLen}s";

			switch(stg.Type) {
				// silencedetect write-ffmpeg-script
				case RunnerType.WriteFFMpegSilenceScript: { //silence
					AlbumToTracksInfo ati = new(stg.Directory) {
						LogFolderName = logsNm,
						RemoveRootDirFromScript = false, 
					};

					await ati.RUN(silenceInSecs: silenceLen, runProcess: false);

					string script = ati.Scripts;

					break;
				}
				// silencedetect tocsv
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

public class SilenceDetectRunSettingsOld
{
	public RunnerType Type { get; set; }

	public double[] SilenceLengths { get; set; }

	public string Directory { get; set; }


	public static SilenceDetectRunSettingsOld TryParse(string content, string directory)
	{
		if(content.IsNulle() || !content.Contains('{'))
			return null;

		try {
			SilenceDetectRunSettingsOld stg = content.DeserializeJson<SilenceDetectRunSettingsOld>();
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

public class SilenceDetectRunSettings
{
	public RunnerType Type { get; set; }

	public double[] SilenceLengths { get; set; }

	public string Directory { get; set; }

	public string AudioFilesSearch { get; set; } = "*.mp3";
}
