namespace uREPL
{

#if NET_4_6

static public class Mono
{
	static global::Mono.CSharp.Evaluator evaluator;

	static public System.IO.StringWriter MessageOutput
	{
		get { return null; }
		set	{}
	}

	static public void Initialize()
	{
		var settings = new global::Mono.CSharp.CompilerSettings();
		var printer = new global::Mono.CSharp.ConsoleReportPrinter();
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

#else 

static public class Mono
{
	static public System.IO.TextWriter MessageOutput
	{
		get { return global::Mono.CSharp.Evaluator.MessageOutput; }
		set { global::Mono.CSharp.Evaluator.MessageOutput = value; }
	}

	static public void Initialize()
	{
	}

	static public void ReferenceAssembly(System.Reflection.Assembly asm)
	{
		global::Mono.CSharp.Evaluator.ReferenceAssembly(asm);
	}

	static public object Evaluate(string input)
	{
		return global::Mono.CSharp.Evaluator.Evaluate(input);
	}

	static public string Evaluate(string input, out object result, out bool result_set)
	{
		return global::Mono.CSharp.Evaluator.Evaluate(input, out result, out result_set);
	}

	static public bool Run(string input)
	{
		return global::Mono.CSharp.Evaluator.Run(input);
	}

	static public string[] GetCompletions(string input, out string prefix)
	{
		return global::Mono.CSharp.Evaluator.GetCompletions(input, out prefix);
	}

	static public string GetVars()
	{
		return global::Mono.CSharp.Evaluator.GetVars();
	}

	static public string GetUsing()
	{
		return global::Mono.CSharp.Evaluator.GetUsing();
	}
}

#endif

}