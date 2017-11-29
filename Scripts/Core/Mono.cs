using System.IO;
using System.Text;

namespace uREPL
{

public class MonoMessageReporter : TextWriter
{
    public string lastOutput { get; private set; }

    public override void Write(string value)
    {
        lastOutput = value;
    }

    public override void WriteLine(string value)
    {
        lastOutput = value;
    }

    public void Reset()
    {
        lastOutput = "";
    }

    public override Encoding Encoding
    {
        get { return Encoding.Default; }
    }
}

static public class Mono
{
	static global::Mono.CSharp.Evaluator evaluator;
	static MonoMessageReporter reporter = new MonoMessageReporter();
	static public string lastOutput
	{
		get
		{
			var output = reporter.lastOutput;
			reporter.Reset();
			return output;
		}
	}

	static public void Initialize()
	{
		var settings = new global::Mono.CSharp.CompilerSettings();
		var printer = new global::Mono.CSharp.ConsoleReportPrinter(reporter);
		var context = new global::Mono.CSharp.CompilerContext(settings, printer);
		evaluator = new global::Mono.CSharp.Evaluator(context);
	}

	static public void ReferenceAssembly(System.Reflection.Assembly asm)
	{
		evaluator.ReferenceAssembly(asm);
	}

	static public object Evaluate(string input)
	{
		return evaluator.Evaluate(input);
	}

	static public string Evaluate(string input, out object result, out bool result_set)
	{
		return evaluator.Evaluate(input, out result, out result_set);
	}

	static public bool Run(string input)
	{
		return evaluator.Run(input);
	}

	static public string[] GetCompletions(string input, out string prefix)
	{
		return evaluator.GetCompletions(input, out prefix);
	}

	static public string GetVars()
	{
		return evaluator.GetVars();
	}

	static public string GetUsing()
	{
		return evaluator.GetUsing();
	}
}

}