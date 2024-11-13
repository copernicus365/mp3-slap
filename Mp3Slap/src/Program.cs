using Mp3Slap;

using static System.Console;

Mp3ToSplitPathsInfo a1 = AlbumToTracksInfo.GetMp3ToSplitPathsInfo(path: "C:/Dropbox/Vids/mp3-split/mp3-split/test1/NIV-Suchet-01-Genesis.mp3", "log5");

Mp3ToSplitPathsInfo a2 = AlbumToTracksInfo.GetMp3ToSplitPathsInfo(path: "C:/Dropbox/Vids/mp3-split/mp3-split/test1/NIV-Suchet-27-Daniel.mp3", "log5");

//"C:/repos/mp3-slap/Test/data/sample-ffmpeg-silence-logs/"

//NIV-Suchet-27-Daniel.mp3
//NIV-Suchet-01-Genesis.mp3

//return;

WriteLine("mp3 splitter!");

string dir = "/Users/nikos/repos/praxis/mp3-split/aud/";
dir = "/Users/nikos/repos/praxis/mp3-split/messiah";
dir = "C:/Dropbox/Vids/mp3-split/mp3-split/test1";

AlbumToTracksInfo ati = new(dir) {
	LogFolderName = "logs5",
	VerboseScript = false,
	WriteCsvs = false,
	RemoveRootDirFromScript = true,
};

await ati.RUN(silenceInSecondsMin: 2.0, runProcess: false);

string script = ati.Scripts;

WriteLine("end mp3 splitter!");
