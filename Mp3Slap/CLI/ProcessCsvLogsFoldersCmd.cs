using System.CommandLine;

using CommandLine.EasyBuilder;

using Mp3Slap.SilenceDetect;

namespace Mp3Slap.CLI.SilenceDetect;

[Command(
	"csvs",
	Alias = "process-sd-csv-group",
	Description = "Reprocesses silence detect csv logs[1] and possibly (re)generating Adobe Audition marker csv files ([1] e.g. `log#foo1.mp3#silencedetect-parsed.csv`).")]
public class ProcessCsvLogsFoldersCmd : SilenceDetectBase
{
	[Option("--adds-subs-only",
		alias: "-addsub",
		description: "True to immediately ignore any instances where no substractions or adds were found without the source silence-detect csvs.",
		DefVal = false)]
	public bool IgnoreAllWOutAddsOrPluses { get; set; }


	public async Task HandleAsync()
	{
		SilenceDetectFullFolderArgs args = new() {
		};

		if(!SetArgs(args))
			return;

		SilenceDetectFullFolderScript[] res = await SilenceDetectFullFolderScript.RunManyDurations(
			args,
			async script => {
				ProcessSilDetCSV p = new() {
					IgnoreAllWOutAddsOrPluses = IgnoreAllWOutAddsOrPluses, 
				};
				await p.RUN(script.Infos);
			});
	}
}
