using Mp3Slap;

using static System.Console;

WriteLine("mp3 splitter!");

string dir = "C:/Dropbox/Vids/mp3-split/mp3-split/test1";

double silenceLen = 2.2;

string logsNm = $"logs1-{silenceLen}s";

"Enter '1' or 't' to generate raw script that can be ran in bash to generate the raw ffmpeg silence files. Else anything else to PARSE them into csvs.".Print();

ConsoleKey ky = Console.ReadKey().Key;

bool writeRaw = ky == ConsoleKey.D1 || ky == ConsoleKey.T;

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


return;







//string dir = "/Users/nikos/repos/praxis/mp3-split/aud/";
//dir = "/Users/nikos/repos/praxis/mp3-split/messiah";
