namespace Mp3Slap.SilenceDetect;

public class MegaSilenceDetectScriptsHub
{
	public async Task RUN(MegaSilenceDetectArgs args)
	{
		double[] durations = args.SilenceDurations;

		for(int i = 0; i < durations.Length; i++) {
			double silenceDur = durations[i];

			MegaSilenceDetectScript ati = new(args, silenceDur);

			await ati.RUN();

			string script = ati.Scripts;

			//FFMpegSilenceLogToCSVConverter conv = new(null);
			//FFMpegSilenceLogToCSVConverter.ConvertFFMpegSilenceLogsToCSVs(
			//	logsDir: logsDir,
			//	srcDir: null,
			//	pad: csvArgs.Pad); // // silenceDur);

		}
	}
}
