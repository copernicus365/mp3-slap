using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.General;

namespace Mp3Slap.CLI;

[Command("test1", "Just testing ...")]
public class Test1Cmd
{
	[Option("--ffverbose", "-v", "FFMpeg verbose setting", DefVal = "quiet")]
	public string FFVerboseMode { get; set; }

	[Option("--pause", "-p", "Pause in seconds before each run of ffmpeg script", DefVal = 0.5)]
	public double Pause { get; set; }

	public async Task Handle()
	{
		"hi!".Print();

		SSDur[] durs = [
			new("0:05:47.50", "0:03:52.09", 2),
			new("0:09:39.59", "0:04:13.99", 3),
			new("0:13:53.59", "0:04:18.28", 4),
			new("0:18:11.87", "0:03:54.18", 5),
		];

		//string args =
		//	""" -ss "0:05:47.50" -i "gen.mp3" -vcodec copy -acodec copy -to "0:03:52.09" "out/gen-02.mp3" """;
		//args = """ -ss "0:09:39.59" -i "gen.mp3" -vcodec copy -acodec copy -to "0:04:13.99" "out/gen-03.mp3" """;

		TimeSpan delay = Pause <= 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(Pause);

		StringBuilder sb = new(6000);

		List<(SSDur dur, string ffOutput)> results = [];

		for(int i = 0; i < durs.Length; i++) {

			SSDur d = durs[i];
			int num = d.num;

			string args = $""" -ss "{d.start}" -i "gen.mp3" -vcodec copy -acodec copy -to "{d.duration}" "out9/gen-{num:00}.mp3" """.Trim();

			//if(FFVerboseMode.NotNulle())
			//	args += " -v " + FFVerboseMode;

			//args = """ -i gen.mp3 -af silencedetect=noise=-30dB:d=2.8 -f null - """.Trim();

			$"\n\nrun ffmpeg split: {i,00} - num: {num,00}\n\n".Print();

			if(Pause > 0)
				await Task.Delay(delay);

			var si = new ProcessStartInfo {
				FileName = "ffmpeg",
				Arguments = args,
				UseShellExecute = false,
				CreateNoWindow = false,
			};

			string foutput = await ProcessHelperX.RunFFMpegProcess(args);

				//si.Start_GetString(TimeSpan.FromMilliseconds(50));

			results.Add((d, foutput));
		}

		"done?".Print();
	}

}

record SSDur(TimeSpan start, TimeSpan duration, int num)
{
	public SSDur(string startS, string durationS, int num = 0)
		: this(TimeSpan.Parse(startS), TimeSpan.Parse(durationS), num) { }
};
