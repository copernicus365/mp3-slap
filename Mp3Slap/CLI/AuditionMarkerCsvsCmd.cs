using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"audition-marker-csvs",
	Alias = "aud-csv",
	Description = "Converts silence detect csvs[1] to Adobe Audition marker csv files ([1] e.g. `log#foo1.mp3#silencedetect-parsed.csv`).")]
public class AuditionMarkerCsvsCmd : SilenceDetectBase
{
	public async Task HandleAsync()
	{
		MegaSilenceDetectArgs args = new() {
		};

		if(!SetArgs(args))
			return;

		MegaSilenceDetectScript[] res = await MegaSilenceDetectScript.RunManyDurations(
			args,
			async script => {
				WriteAuditionMarkerCSVs wcsv = new();
				await wcsv.RUN(script.Infos);
			});
	}
}
