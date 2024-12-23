using System;
using System.Text;

namespace Mp3Slap.SilenceDetect;

/// <summary>
/// A SINGLE silence detect script, ie on a singular audio file, etc.
/// </summary>
public class SilDetectWriteFFScript
{
	public StringBuilder SB { get; private set; } = new();

	public string Scripts { get; private set; }

	public string EchoFirstLine { get; private set; }

	public string SetScripts()
		=> Scripts = SB.ToString();

	public void StartScript()
	{
		SB = new();
		SB
			.AppendLine("#!/bin/sh")
			.AppendLine()
			.AppendLine("echo \"+++FFMpeg Silence Detect Script +++\"")
			.AppendLine();
	}

	public void WriteFinalCombinedFFMpegShellScriptsToFile(
		Mp3ToSplitPathsInfo info,
		string scriptPth,
		bool writeRelativePaths,
		bool save = true)
	{
		SetScripts();

		if(writeRelativePaths) {
			Scripts = Scripts
				.Replace(info.LogDirectory, "./")
				.Replace(info.Directory, "./../");
		}
		if(save)
			File.WriteAllText(scriptPth, Scripts);
	}

	public void AddScipt(
		SilDetScripts Script,
		Mp3ToSplitPathsInfo info,
		bool verbose,
		int index = 0)
	{
		EchoFirstLine = $"i: {index,2} run silence detect on: '{info.AudioFileName}'";

		string script = $"""
echo "--- {EchoFirstLine} ---"

echo "log: '{info.SDTimeStampsCSVPath}"

sleep 2

{Script.FFMpegScript}
""";
		script = script.Trim('"'); // ?! can't get """ type string to not put "quotes"!

		SB ??= new();

		if(verbose) {
			SB
				.AppendLine(script)
				.AppendLine();
		}
		else {
			SB
				.AppendLine(Script.GetFFMpegScript(withLogPaths: true))
				.AppendLine();
		}
	}
}
