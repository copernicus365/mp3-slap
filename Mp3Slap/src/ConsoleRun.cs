using static System.Console;

using CommandLine;

namespace Mp3Slap;

public class ConsoleRun
{
	public static async Task Main(string[] args)
	{
		while(true) {

			if(args.IsNulle()) {
				Write("Input: ");
				string input = ReadLine().NullIfEmptyTrimmed();
				if(input == null) {
					WriteLine("  empty input...");
					continue;
				}
				args = [input];
			}

			RUN2(args);
			args = null;
		}
	}

	public static void RUN2(string[] args)
	{
		////// (1) default options
		////var result = Parser.Default.ParseArguments<Options>(args);
		// or (2) build and configure instance
		var parser = new Parser(with => {
			with.EnableDashDash = true;
		});
		ParserResult<Options> result = parser.ParseArguments<Options>(args);
		var val = result.Value;
		Console.WriteLine("Hello World.");
	}

	public static async Task<int> RUN(string[] args)
	{

		ParserResult<object> pRes = Parser.Default.ParseArguments<AddOptions, CommitOptions, CloneOptions>(args)
			.WithParsed<AddOptions>(opts => opts.Print())
			.WithParsed<CommitOptions>(opts =>
				opts.Print())
			.WithParsed<CloneOptions>(opts => opts.Print())
			.WithNotParsed(errors => errors.Print());

		return 1;
		//return Parser.Default.ParseArguments<AddOptions, CommitOptions, CloneOptions>(args)
		//	.MapResult(
		//		(AddOptions opts) => opts.Print(), // RunAddAndReturnExitCode(opts),
		//		(CommitOptions opts) => opts.Print(), // RunCommitAndReturnExitCode(opts),
		//		(CloneOptions opts) => opts.Print(), //RunCloneAndReturnExitCode(opts),
		//		errs => 1);
	}
}

class Options
{
	[Value(0)]
	public int IntValue { get; set; }

	[Value(1, Min = 1, Max = 3)]
	public IEnumerable<string> StringSeq { get; set; }

	[Value(2)]
	public double DoubleValue { get; set; }
}


[Verb("add", HelpText = "Add file contents to the index.")]
class AddOptions
{

	//normal options here
}

[Verb("commit", HelpText = "Record changes to the repository.")]
class CommitOptions
{
	[Value(0)]
	public string Path { get; set; }

	[Option(Required = true, HelpText = "Cool u dawg..")]
	public string UserId { get; set; }

	[Option(Required = false, HelpText = "Name man..")]
	public string LName { get; set; }

	//commit options here
}

[Verb("clone", HelpText = "Clone a repository into a new directory.")]
class CloneOptions
{
	//clone options here
}
