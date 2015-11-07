using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using Mono.CSharp;

namespace uREPL
{

public class CompletionInfo
{
	public string prefix;
	public string code;
	public string mark;
	public Color32 color;

	public CompletionInfo(
		string prefix,
		string code,
		string mark,
		Color32 color)
	{
		this.prefix = prefix;
		this.code   = code;
		this.mark   = mark;
		this.color  = color;
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
}


public class MonoCompletion : CompletionPlugin
{
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
				prefix,
				completion.Replace(prefix, ""),
				"M",
				new Color32(50, 70, 240, 255)))
			.ToArray();
	}
}


public class CommandCompletion : CompletionPlugin
{
	static public CommandCompletion instance;
	static private CommandInfo[] commands;

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
					input,
					x.command.Replace(input, ""),
					"C",
					new Color32(200, 50, 30, 255)))
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
					partialPath,
					x.Replace(partialPath, ""),
					"P",
					new Color32(30, 200, 80, 255)))
				.ToArray();
		}
		return pathCompletions;
	}
}


}
