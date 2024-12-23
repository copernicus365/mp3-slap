namespace Mp3Slap.SilenceDetect;

public class SilenceDetectRunSingle(
	Mp3ToSplitPathsInfo info,
	SilenceDetectFullFolderArgs args,
	SilDetectWriteFFScript writeScript,
	int index = 0)
{
	public readonly SilDetScripts Script
		= new SilDetScripts().Init(info, info.SilenceDuration);

	bool verbose => args.Verbose;

	public async Task RUN()
	{
		if(index == 0)
			$"-- {(args.RunFFScript ? "RUN" : "Calculate")} Silence Detect - dur: {info.SilenceDuration}".Print();

		writeScript.AddScipt(Script, info, verbose, index);

		if(verbose)
			writeScript.EchoFirstLine.Print();

		if(!args.RunFFScript)
			return;

		string ffSplitOutput = await ProcessHelperX.RunFFMpegProcess(Script.FFMpegScriptArgsNoLogPath);

		if(args.WriteFFMpegSilenceLogs)
			File.WriteAllText(info.FFSDLogPath, ffSplitOutput);

		WriteFFMpegSilDetLogToCsvs ww = new() {
			Pad = args.Pad,
			WriteAuditionMarkerCsvs = args.WriteAuditionMarkerCsvs,
		};

		await ww.RUN(info, ffSplitOutput);

		SilDetTimeStampsMeta meta = ww.CSV.Meta;

		if(verbose)
			$"- count: {meta.count} | total-dur: {meta.duration}".Print();

	}
}
