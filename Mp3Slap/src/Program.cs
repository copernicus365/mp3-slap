using Mp3Slap;

using static System.Console;

WriteLine("mp3 splitter!");

string dir = "/Users/nikos/repos/praxis/mp3-split/aud/";
dir = "/Users/nikos/repos/praxis/mp3-split/messiah";
dir = "C:/Dropbox/Vids/mp3-split/mp3-split/test1";

AlbumToTracksInfo ati = new(dir) {
	LogFolderName = "logs4",
	WriteCsvsOnly = true,
};

await ati.RunSilenceDetectScript_1(silenceInSecondsMin: 2.0, runProcess: true);

WriteLine("end mp3 splitter!");
