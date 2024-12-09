using System.Diagnostics;

namespace Mp3Slap.General;

public static class ProcessHelperX
{
	public static async Task<string> RunFFMpegProcess(string args)
	{
		ProcessStartInfo si = new() {
			FileName = "ffmpeg",
			Arguments = args,
			UseShellExecute = false,
			CreateNoWindow = false,
		};

		string output = await si.Start_GetString(TimeSpan.FromMilliseconds(50));
		return output;
	}
}
