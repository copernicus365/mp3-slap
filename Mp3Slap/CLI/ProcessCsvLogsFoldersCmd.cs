using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"csv-logs",
	Alias = "csvs",
	Description = "Reprocesses silence detect csv logs[1] and possibly (re)generating Adobe Audition marker csv files ([1] e.g. `log#foo1.mp3#silencedetect-parsed.csv`).")]
public class ProcessCsvLogsFoldersCmd : SilenceDetectBase
{
	public async Task HandleAsync()
	{
		SilenceDetectFullFolderArgs args = new() {
		};

		if(!SetArgs(args))
			return;

		SilenceDetectFullFolderScript[] res = await SilenceDetectFullFolderScript.RunManyDurations(
			args,
			async script => {
				ProcessSilDetCSV wcsv = new();
				await wcsv.RUN(script.Infos);
			});
	}
}
