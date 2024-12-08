using System.Text;

namespace Mp3Slap.SilenceDetect;

public class WriteMp3SplitterScript
{
	public string Dir { get; set; }

	public string RemovePrefix { get; set; }

	public string SrcFile { get; set; }

	public string NameNoExt { get; private set; }

	public string Ext { get; private set; }

	public string ExtrDir { get; set; }

	public string AlbumPath { get; set; }

	public void INIT()
	{
		SrcFile = PathHelper.CleanPath(SrcFile);

		string nameNoExt = Path.GetFileNameWithoutExtension(SrcFile);

		Ext = Path.GetExtension(SrcFile);

		NameNoExt = nameNoExt.Replace(RemovePrefix, "").NullIfEmptyTrimmed();

		AlbumPath = PathHelper.CleanDirPath(Path.Combine(Dir, ExtrDir, NameNoExt));

		if(!Directory.Exists(AlbumPath))
			Directory.CreateDirectory(AlbumPath);
	}

	public void RUN(TrackTimeStamp[] stamps)
	{
		string digCount = stamps.Length.DigitsCountFrmt();

		List<string> scripts = new(stamps.Length);

		for(int i = 0; i < stamps.Length; i++) {

			TrackTimeStamp stamp = stamps[i];

			int num = i + 1;
			string fileName = $"{NameNoExt}-{num.ToString(digCount)}{Ext}";
			string fullPath = PathHelper.CleanPath(Path.Combine(AlbumPath, fileName));

			string script = GetScript(stamp.PaddedStart, stamp.PaddedDuration, SrcFile, fileName, fullPath);
			scripts.Add(fullPath);
		}

		string scriptCont = scriptSB.ToString();

		string scriptPath = AlbumPath + $"split-mp3-script-{NameNoExt}.sh";
		File.WriteAllText(scriptPath, scriptCont);
	}

	StringBuilder scriptSB = new StringBuilder()
		.AppendLine("#!/bin/sh")
		.AppendLine();

	public string GetScript(
		TimeSpan start,
		TimeSpan duration,
		string srcMp3,
		string name,
		string file)
	{
		string script =
			$"""
			echo "splitting "{name}"

			ffmpeg -ss "{start.ToString(TrackTimeStamp.TSFrmt)}" -i "{srcMp3}" -vcodec copy -acodec copy -to "{duration.ToString(TrackTimeStamp.TSFrmt)}" "{file}"

			""";
		scriptSB.AppendLine(script);
		return script;
		//"ffmpeg -ss "0:05:47.50" -i gen.mp3 -vcodec copy -acodec copy -to "0:03:52.08" test/01-gen.mp3
	}

	void dd()
	{
		StringBuilder sb = new();
		sb
			.AppendLine("#!/bin/sh")
			.AppendLine()
			.AppendLine("echo \"+++FFMpeg Silence Detect Script +++\"")
			.AppendLine();

	}
}
