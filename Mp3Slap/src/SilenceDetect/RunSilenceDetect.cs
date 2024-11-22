using static System.Console;

namespace Mp3Slap.SilenceDetect;

public class RunSilenceDetect
{
	public async Task RUN(RunnerType type, SilenceDetectArgsBase args)
	{
		double[] silenceLens = args.SilenceDurations;
		// [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];

		for(int i = 0; i < silenceLens.Length; i++) {
			double silenceLen = silenceLens[i];

			string logsNm = args.GetLogFolderName(silenceLen, fullPath: false); // $"logs1-{silenceLen}s";

			switch(type) {
				case RunnerType.WriteFFMpegSilenceScript: {

					var fargs = args as SilenceDetectWriteFFMpegScriptArgs;

					AlbumToTracksInfo ati = new(fargs.Directory) {
						LogFolderName = logsNm,
						RemoveRootDirFromScript = fargs.WriteRelativePaths,
						VerboseScript = fargs.Verbose ?? false,
					};

					await ati.RUN(silenceInSecs: silenceLen, runProcess: false);

					string script = ati.Scripts;

					break;
				}
				// silencedetect tocsv
				case RunnerType.ConvertFFMpegSilenceLogsToCSVs: {

					var fargs = args as ConvertFFMpegSilenceLogsToCSVArgs;

					// write csvs from raw results
					RunParserOnLogsFolder ff = new();
					ff.RUN(logsDir: args.Directory + '/' + logsNm);
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
