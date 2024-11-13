namespace Mp3Slap;

public class RunParserOnLogsFolder
{
	public void RUN(string logsDir, string srcDir = null, double pad = 3)
	{
		string[] logsPaths = PathHelper.GetFilesFromDirectory(logsDir, "log", includeSubDirectories: false);

		TrackTimeStampsCsv first = null;

		for(int i = 0; i < logsPaths.Length; i++) {
			string path = logsPaths[i];

			TrackTimeStampsCsv tcsv = LogFileNames.GetPathsEtc(path, srcDir);
			if(tcsv == null)
				continue;

			if(first == null) {
				first = tcsv;
				srcDir = first.SrcDir;
			}

			FFSilenceTracksParser ffP = AlbumLogsWriter.GetAndWriteCsvParsed(tcsv);

			//TrackTimeStampsCsv csv = new();
			//csv.Parse(csvText);

			//True(csv.Count == 17);
			//csv.CombineCuts();

			//True(csv.Valid);
			//True(csv.Count == 10);
		}
	}
}
