using static System.Console;

namespace Mp3Slap.SilenceDetect;

public enum RunnerType
{
	None = 0,
	WriteFFMpegSilenceScript = 1,
	ConvertFFMpegSilenceLogsToCSVs = 2,
}

public class RunSilenceDetect
{
	public async Task RUN(RunnerType type, SilenceDetectArgsBase args)
	{
		double[] durations = args.SilenceDurations;

		for(int i = 0; i < durations.Length; i++) {
			double silenceDur = durations[i];

			switch(type) {
				case RunnerType.WriteFFMpegSilenceScript: {

					var sdargs = args as SilenceDetectWriteFFMpegScriptArgs;

					WriteFFMpegSilenceDetectScript ati = new(sdargs, silenceDur);

					await ati.RUN();

					string script = ati.Scripts;

					break;
				}
				case RunnerType.ConvertFFMpegSilenceLogsToCSVs: {

					var fargs = args as ConvertFFMpegSilenceLogsToCSVArgs;

					string logsNm = args.GetLogFolderName(silenceDur, fullPath: false);

					// write csvs from raw results
					RunParserOnLogsFolder ff = new();
					ff.RUN(logsDir: args.Directory + '/' + logsNm);

					break;
				}
			}
		}
	}
}
