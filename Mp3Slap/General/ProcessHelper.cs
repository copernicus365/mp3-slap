using System.Diagnostics;

namespace Mp3Slap.General;

public static class ProcessHelper
{
	public static async Task<string> Start_GetString(
		this ProcessStartInfo startInfo,
		TimeSpan delay = default)
	{
		var lines = await startInfo.Start_GetLines(delay);
		string output = lines.JoinToString(v => v.line, "\n");
		return output;
	}

	public static async Task<List<(string line, bool error)>> Start_GetLines(
		this ProcessStartInfo startInfo,
		TimeSpan delay = default,
		List<(string line, bool error)> outputLines = null)
	{
		outputLines ??= [];

		startInfo.RedirectStandardOutput = true;
		startInfo.RedirectStandardError = true;

		Process p = new() {
			StartInfo = startInfo,
		};

		p.OutputDataReceived += (sender, args) => outputLines.Add((args.Data, false));
		p.ErrorDataReceived += (sender, args) => outputLines.Add((args.Data, true));

		p.Start();
		p.BeginOutputReadLine();
		p.BeginErrorReadLine();

		bool hasDelay = delay > TimeSpan.Zero;

		while(!p.HasExited) {
			if(hasDelay)
				await Task.Delay(delay);
		}

		return outputLines;
	}

	public static async Task Run(string procOrFileName, string cmdOrArgs)
	{
		cmdOrArgs = cmdOrArgs.Replace("'", "").Replace("2>", ">");
		ProcessStartInfo info = new(procOrFileName, cmdOrArgs);
		Process p = Process.Start(info);
		await p.WaitForExitAsync();
	}
}
