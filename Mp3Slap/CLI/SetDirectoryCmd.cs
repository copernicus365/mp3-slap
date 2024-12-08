using CommandLine.EasyBuilder.Auto;

using Mp3Slap.General;

namespace Mp3Slap.CLI;

[Command(
	"current-directory",
	Alias = "cd",
	Description = "Sets the current environment directory for the app globally")]
public class SetDirectoryCmd
{
	[Argument("value", description: "Directory path to set as root.")]
	public DirectoryInfo Dir { get; set; }

	public void Handle()
	{
		DirectoryInfo d = Dir;

		if(d == null || !d.Exists) {
			$"Directory does not exist".Print();
			return;
		}

		string path = PathHelper.CleanDirPath(d.FullName);

		if(!Directory.Exists(path)) {
			$"Directory does not exist..".Print();
			return;
		}

		string origDir = Program.CurrentDirectory;

		Environment.CurrentDirectory =
			Program.CurrentDirectory =
			path;

		$@"Changed root directory:
old: {origDir}
new: {Program.CurrentDirectory}".Print();
	}
}

[Command("print", description: "Prints current directory")]
public class PrintCurrDirectoryCmd
{
	public void Handle()
	{
		$@"Current root directory:
app: {Program.CurrentDirectory}
env: {Environment.CurrentDirectory}".Print();
	}
}
