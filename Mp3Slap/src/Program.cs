using static System.Console;
using Mp3Slap;

WriteLine("mp3 splitter!");

string dir = "/Users/nikos/repos/praxis/mp3-split/aud/";
dir = "/Users/nikos/repos/praxis/mp3-split/messiah";

AlbumToTracksInfo ati = new(dir);

await ati.RunSilenceDetectScript_1(silenceInSecondsMin: 3.5, runProcess: true);

WriteLine("end mp3 splitter!");
