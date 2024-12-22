using Mp3Slap.SilenceDetect;

namespace Test;

public class SilenceDetectArgsBaseTests : BaseTest
{
	[Fact]
	public void Standard_NotRootedLogFolder_RoundDur2Dec()
	{
		SilenceDetectArgs args = new() {
			Directory = "C:\\Temp\\test1",
			LogFolder = "logs1",
			SilenceDurations = [3.1]
		};

		True(args.INIT().Success);

		double dur = 3.237;

		string folderPathL = args.GetLogFolderName(dur, fullPath: false);
		string folderPathF = args.GetLogFolderName(dur, fullPath: true);

		True(args.Directory == "C:/Temp/test1/");
		True(folderPathL == "logs1-3.24/");
		True(folderPathF == "C:/Temp/test1/logs1-3.24/");
	}

	[Fact]
	public void ResolveLogPathRelativeNav()
	{
		SilenceDetectArgs args = new() {
			Directory = "C:\\Temp\\test1",
			LogFolder = "..\\logs1",
			SilenceDurations = [ 3.1 ]
		};

		True(args.INIT().Success);

		double dur = 3.2;

		string folderPathL = args.GetLogFolderName(dur, fullPath: false);
		string folderPathF = args.GetLogFolderName(dur, fullPath: true);

		True(args.Directory == "C:/Temp/test1/");
		True(folderPathL == "../logs1-3.2/");
		True(folderPathF == "C:/Temp/logs1-3.2/");
	}

	[Fact]
	public void RootedLogFolder1()
	{
		SilenceDetectArgs args = new() {
			Directory = "C:\\Temp\\test1",
			LogFolder = "\\hi1\\logs1",
			SilenceDurations = [3.1]
		};

		True(args.INIT().Success);

		double dur = 3;

		string folderPathL = args.GetLogFolderName(dur, fullPath: false);
		string folderPathF = args.GetLogFolderName(dur, fullPath: true);

		True(args.Directory == "C:/Temp/test1/");
		True(folderPathF == "/hi1/logs1-3/");
		True(folderPathF == folderPathL);
	}

	[Fact]
	public void Error_DirectoryNotAbsolute()
	{
		SilenceDetectArgs args = new() {
			Directory = "Temp\\test1",
			LogFolder = "\\hi1\\logs1",
			SilenceDurations = [3.1]
		};

		SResult res = args.INIT();
		False(res.Success);
		True(res.Message.ContainsIgnoreCase("not rooted"));
	}

	[Fact]
	public void FixDirectoryNav()
	{
		SilenceDetectArgs args = new() {
			Directory = "C:\\Temp\\test1\\..\\",
			LogFolder = "hi1\\logs1",
			SilenceDurations = [2]
		};

		SResult res = args.INIT();
		True(res.Success);

		double dur = 3;

		string folderPathL = args.GetLogFolderName(dur, fullPath: false);
		string folderPathF = args.GetLogFolderName(dur, fullPath: true);

		True(args.Directory == "C:/Temp/");
		True(folderPathF == "C:/Temp/hi1/logs1-3/");
		True(folderPathL == "hi1/logs1-3/");
	}
}
