namespace Mp3Slap.SilenceDetect;

public enum RunnerType
{
	None = 0,
	WriteFFMpegSilenceScript = 1,
	ConvertFFMpegSilenceLogsToCSVs = 2,
	WriteSplitScript = 3,
}

public class RunSilenceDetect
{
	public async Task RUN(RunnerType type, SilenceDetectArgs args)
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

					var csvArgs = args as ConvertFFMpegSilenceLogsToCSVArgs;
					string logsDir = args.GetLogFolderName(silenceDur, fullPath: true);

					FFMpegSilenceLogToCSVConverter.ConvertFFMpegSilenceLogsToCSVs(
						logsDir: logsDir,
						srcDir: null,
						pad: csvArgs.Pad);

					break;
				}
				case RunnerType.WriteSplitScript: {
					WriteMp3SplitterScript script = new() {
						 
					};
					break;
				}
			}
		}
	}
}
