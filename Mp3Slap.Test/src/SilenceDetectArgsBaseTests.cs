using Mp3Slap.SilenceDetect;

namespace Test;

public class SilenceDetectArgsBaseTests : BaseTest
{
	[Fact]
	public void Standard_NotRootedLogFolder()
	{
		SilenceDetectArgs d1 = new() {
			Directory = "C:\\Temp\\test1",
			LogFolder = "logs1-{duration}s",
		};

		d1.INIT();

		double dur = 3.237;

		string folderPathL = d1.GetLogFolderName(dur, fullPath: false);
		string folderPathF = d1.GetLogFolderName(dur, fullPath: true);

		True(d1.Directory == "C:/Temp/test1/");
		True(folderPathL == "logs1-3.24s/");
		True(folderPathF == "C:/Temp/test1/logs1-3.24s/");
	}

	[Fact]
	public void ResolveLogPathRelativeNav()
	{
		SilenceDetectArgs d1 = new() {
			Directory = "C:\\Temp\\test1",
			LogFolder = "..\\logs1-{duration}s",
		};

		d1.INIT();

		double dur = 3.2;

		string folderPathL = d1.GetLogFolderName(dur, fullPath: false);
		string folderPathF = d1.GetLogFolderName(dur, fullPath: true);

		True(d1.Directory == "C:/Temp/test1/");
		True(folderPathL == "../logs1-3.2s/");
		True(folderPathF == "C:/Temp/logs1-3.2s/");
	}

	[Fact]
	public void RootedLogFolder1()
	{
		SilenceDetectArgs d1 = new() {
			Directory = "C:\\Temp\\test1",
			LogFolder = "\\hi1\\logs1-{duration}s",
		};

		d1.INIT();

		double dur = 3;

		string folderPathL = d1.GetLogFolderName(dur, fullPath: false);
		string folderPathF = d1.GetLogFolderName(dur, fullPath: true);

		True(d1.Directory == "C:/Temp/test1/");
		True(folderPathF == "/hi1/logs1-3s/");
		True(folderPathF == folderPathL);
	}

	[Fact]
	public void Error_DirectoryNotAbsolute()
	{
		SilenceDetectArgs d1 = new() {
			Directory = "Temp\\test1",
			LogFolder = "\\hi1\\logs1-{duration}s",
		};

		SResult res = d1.INIT();
		False(res.Success);
		True(res.Message.ContainsIgnoreCase("not rooted"));
	}

	[Fact]
	public void FixDirectoryNav()
	{
		SilenceDetectArgs d1 = new() {
			Directory = "C:\\Temp\\test1\\..\\",
			LogFolder = "hi1\\logs1-{duration}s",
			SilenceDurations = [2]
		};

		SResult res = d1.INIT();
		True(res.Success);

		double dur = 3;

		string folderPathL = d1.GetLogFolderName(dur, fullPath: false);
		string folderPathF = d1.GetLogFolderName(dur, fullPath: true);

		True(d1.Directory == "C:/Temp/");
		True(folderPathF == "C:/Temp/hi1/logs1-3s/");
		True(folderPathL == "hi1/logs1-3s/");
	}
}
