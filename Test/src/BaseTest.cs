using DotNetXtensions.Test;

namespace Test;

public class BaseTest : XUnitTestBase
{
    public BaseTest(string dataBasePath = "data")
        : base(dataBasePath) { CacheResourceGetsDefault = true; }

    static BaseTest() => INIT_SITE();

    public static void INIT_SITE()
    {
        SetConsoleOutputToTestLog();

        string vsOutLogPath = VsOutLogPath;
    }


    public string FullResourcePath(string after = null)
        => $"{RootProjectDirectory}{ResourceBasePath.Replace(".", "/")}/{after}";

    public string GetDataDirPath(string extra = null)
        => Path.Combine(RootProjectDirectory, $"data/{extra}");


    protected static void assertEq(string[] arr1, string[] arr2)
    {
        if (arr1.IsNulle() || arr2.IsNulle())
        {
            Assert.True(arr1.IsNulle() && arr2.IsNulle());
            return;
        }

        arr1 = arr1?.Select(v => v.ToLower()).OrderBy(v => v).ToArray();
        arr2 = arr2?.Select(v => v.ToLower()).OrderBy(v => v).ToArray();

        bool match = arr1.SequenceEqual(arr2);

        Assert.True(match);
    }
}
