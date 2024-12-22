namespace Mp3Slap.SilenceDetect;

public class SilenceDetectRunSingle(
	Mp3ToSplitPathsInfo info,
	SilenceDetectFullFolderArgs args,
	SilDetectWriteFFScript writeScript,
	int index = 0)
{
	public readonly SilDetScripts Script
		= new SilDetScripts().Init(info, info.SilenceDuration);

	public async Task RUN()
	{
		writeScript.AddScipt(Script, info, args.Verbose, index);

		if(!args.RunFFScript)
			return;

		string ffSplitOutput = await ProcessHelperX.RunFFMpegProcess(Script.FFMpegScriptArgsNoLogPath);

		if(args.WriteFFMpegSilenceLogs)
			File.WriteAllText(info.SilenceDetectRawLogPath, ffSplitOutput);

		WriteFFMpegSilDetLogToCsvs ww = new() {
			Pad = args.Pad,
			WriteAuditionMarkerCsvs = args.WriteAuditionMarkerCsvs,
		};

		await ww.RUN(info, ffSplitOutput);
	}
}
