using Mp3Slap.General;
using Mp3Slap.SilenceDetect;

using Test;

namespace Mp3Slap.Test;

public class WriterTests1 : SilenceDetectBase
{
	static TimeSpan pd = TimeSpan.FromSeconds(0.3);

	[Fact]
	public void ParseLargeCsv_WithSubs_Gen()
	{
		SilDetTimeStampsMeta meta = new(pad: 0.3, duration: "0:22:10.00");
		SilDetTimeStampsCSVWriter wr = new();

		wr.InitForWrite(stamps: GetStamps1(), meta);

		string csv = wr.WriteToString(includeCSVHeader: true);

		// {"duration":3.3,"pad":0.3,"meta":{"duration":"04:04:09.8100000","start":0.025056}}

		string writePath = GetDataDirPath($"{SampleResultsDirName}/log#01-gen-short.mp3#silencedetect-parsed.csv");

		bool WriteParsedLogs = false;
		if(WriteParsedLogs)
			File.WriteAllText(writePath, csv);
	}

}
