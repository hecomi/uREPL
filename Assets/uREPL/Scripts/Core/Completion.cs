using System;
using System.Reflection;
using System.Linq;
using Mono.CSharp;

namespace uREPL
{


public enum CompletionType
{
	Mono    = 0,
	Command = 1,
	Path    = 2,
}


public struct CompletionInfo
{
	public CompletionType type;
	public string prefix;
	public string code;

	public CompletionInfo(CompletionType type, string prefix, string code)
	{
		this.type = type;
		this.prefix = prefix;
		this.code = code;
	}
}


public abstract class CompletionPlugin
{
	static public Type[] GetAllCompletionPluginTypes()
	{
		return System.AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(asm => asm.GetTypes())
			.Where(type => type.IsSubclassOf(typeof(CompletionPlugin)))
			.ToArray();
	}

	public abstract void Initialize();
	public abstract CompletionInfo[] Get(string input);
	public abstract string mark  { get; }
	public abstract string color { get; }
}


public class MonoCompletion : CompletionPlugin
{
	public override string mark
	{
		get { return "M"; }
	}

	public override string color
	{
		get { return "#3355bbff"; }
	}

	public override void Initialize()
	{
		// nothing to do.
	}

	public override CompletionInfo[] Get(string input)
	{
		bool isComplemented = false;
		var result = new string[] {};
		string prefix = "";
		int i1, i2;

		// support generic type completion.
		if (!isComplemented) {
			i1 = input.LastIndexOf("<");
			i2 = input.LastIndexOf(">");
			if (i1 > i2 && i2 < input.Length) {
				input = input.Substring(i1 + 1);
				result = Evaluator.GetCompletions(input, out prefix);
				isComplemented = true;
			}
		}

		// support completion inner parenthesis
		if (!isComplemented) {
			i1 = input.LastIndexOf("(");
			i2 = input.LastIndexOf(")");
			if (i1 > i2 && i2 < input.Length) {
				input = input.Substring(i1 + 1);
				result = Evaluator.GetCompletions(input, out prefix);
				isComplemented = true;
			}
		}

		// otherwise
		if (!isComplemented) {
			result = Evaluator.GetCompletions(input, out prefix);
			isComplemented = true;
		}

		return (result == null) ? null : result
			.Select(completion => new CompletionInfo(
				CompletionType.Mono,
				prefix,
				completion.Replace(prefix, "")))
			.ToArray();
	}
}


public class CommandCompletion : CompletionPlugin
{
	static public CommandCompletion instance;
	static private CommandInfo[] commands;

	public override string mark
	{
		get { return "C"; }
	}

	public override string color
	{
		get { return "#bb5533ff"; }
	}

	public override void Initialize()
	{
		instance = this;
		commands = Attributes.GetAllCommands();
	}

	public override CompletionInfo[] Get(string input)
	{
		return (commands == null) ?
			null :
			commands
				.Where(x => x.command.IndexOf(input) == 0)
				.Select(x => new CompletionInfo(
					CompletionType.Command,
					input,
					x.command.Replace(input, "")))
				.ToArray();
	}

	public CommandInfo[] GetCommands()
	{
		return commands;
	}
}


public class GameObjectPathCompletion : CompletionPlugin
{
	static private string[] allGameObjectPaths;

	public override string mark
	{
		get { return "P"; }
	}

	public override string color
	{
		get { return "#33bb55ff"; }
	}

	public override void Initialize()
	{
		allGameObjectPaths = Utility.GetAllGameObjectPaths();
	}

	public override CompletionInfo[] Get(string input)
	{
		var pathCompletions = new CompletionInfo[] {};
		var i1 = input.LastIndexOf("\"/");
		var i2 = input.LastIndexOf("\"");
		if (i1 != -1 && i1 == i2) {
			var partialPath = input.Substring(i1 + 1);
			pathCompletions = allGameObjectPaths
				.Where(x => x.IndexOf(partialPath) == 0)
				.Select(x => new CompletionInfo(
					CompletionType.Path,
					partialPath,
					x.Replace(partialPath, "")))
				.ToArray();
		}
		return pathCompletions;
	}
}


}
