using Mp3Slap;

using static System.Console;

WriteLine("mp3 splitter!");

string dir = "C:/Dropbox/Vids/mp3-split/mp3-split/test1";
dir = "C:/Dropbox/Music/Bible/Suchet-NIV-1Album";

bool writeRaw = false;
//ConsoleKey ky = ReadKey().Key;
//bool writeRaw = ky == ConsoleKey.D1 || ky == ConsoleKey.T;

double[] silenceLens = [2.0, 2.3, 2.5, 2.7, 3.0, 3.3, 3.7];

for(int i = 0; i < silenceLens.Length; i++) {
	double silenceLen = silenceLens[i];

	string logsNm = $"logs1-{silenceLen}s";

	"Enter '1' or 't' to generate raw script that can be ran in bash to generate the raw ffmpeg silence files. Else anything else to PARSE them into csvs.".Print();

	if(writeRaw) {
		AlbumToTracksInfo ati = new(dir) {
			LogFolderName = logsNm,
			RemoveRootDirFromScript = true,
		};

		await ati.RUN(silenceInSecs: silenceLen, runProcess: false);

		string script = ati.Scripts;

		WriteLine("end mp3 splitter!");
	}
	else {
		// write csvs from raw results
		RunParserOnLogsFolder ff = new();
		ff.RUN(logsDir: dir + '/' + logsNm);
	}
}
