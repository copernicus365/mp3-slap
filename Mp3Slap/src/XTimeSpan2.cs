namespace DotNetXtensions;

public static class XTimeSpan2
{
	public static string ToTotalMinutesString2(this TimeSpan ts)
		=> $"{Math.Truncate(ts.TotalMinutes):00}:{ts.Seconds:00}.{ts:ff}";

	//=> $"{((int)ts.TotalMinutes).ToString("00")}:{ts.Seconds.ToString("00")}.{ts.Milliseconds.ToString().SubstringMax(2)}";
}
