using System.Diagnostics;

namespace Mp3Slap;

public class ProcessHelper
{
	public static async Task Run(string procOrFileName, string cmdOrArgs)
	{
		cmdOrArgs = cmdOrArgs.Replace("'", "").Replace("2>", ">");
		ProcessStartInfo info = new(procOrFileName, cmdOrArgs);
		Process p = Process.Start(info);
		await p.WaitForExitAsync();
	}
}
