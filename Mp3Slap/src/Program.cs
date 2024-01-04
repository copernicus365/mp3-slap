using static System.Console;
using Mp3Slap;

WriteLine("mp3 splitter!");

string dir = "/Users/nikos/repos/praxis/mp3-split/aud/";
dir = "/Users/nikos/repos/praxis/mp3-split/messiah";

AlbumToTracksInfo ati = new(dir) { LogFolderName = "logs2" };

await ati.RunSilenceDetectScript_1(silenceInSecondsMin: 2.0, runProcess: true);

WriteLine("end mp3 splitter!");
