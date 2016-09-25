using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace uREPL
{

[AttributeUsage(
	AttributeTargets.Method,
	AllowMultiple = false,
	Inherited = false
)]
public sealed class CommandAttribute : Attribute
{
    public string description;
	public string name;

    public CommandAttribute()
	{
	}
}

public class CommandInfo
{
	public string methodName;
	public string className;
	public string description;
	public string command;
	public MethodInfo method;

	public CommandInfo(
		string className, 
		string methodName, 
		string description, 
		string command, 
		MethodInfo method)
	{
		this.className   = className;
		this.methodName  = methodName;
		this.description = description;
		this.command     = string.IsNullOrEmpty(command) ? methodName : command;
		this.method      = method;
	}

	public bool HasArguments()
	{
		return method.GetParameters().Length > 0;
	}

	public string GetFormat(string[] args)
	{
		return string.Format("{0}.{1}({2});", 
			className, 
			methodName,
			string.Join(", ", args));
	}

	public string GetFormat()
	{
		return string.Format("{0} {1}.{2}{3}({4})", 
			method.ReturnType.Name,
			className, 
			methodName,
			(method.IsGenericMethod ? "<T>" : ""),
			string.Join(", ", method.GetParameters().Select(
				param => (param.ParameterType.Name + " " + param.Name)).ToArray())
		);
	}

	public string GetTaggedFormat()
	{
		return string.Format("{0} {1}. <b>{2}</b>{3}({4})", 
			method.ReturnType.Name,
			className, 
			methodName,
			(method.IsGenericMethod ? "<T>" : ""),
			string.Join(", ", method.GetParameters().Select(
				param => string.Format("{0} <b>{1}</b>",
					param.ParameterType.Name,
					param.Name)
			).ToArray())
		);
	}
}

public static class Commands
{
	private const string helpTextFile = "uREPL/Xmls/Help";
	static private CommandInfo[] commands_;

	[Command(name = "help", description = "show help")]
	static public void ShowHelp()
	{
		var help = Resources.Load(helpTextFile) as TextAsset;
		Log.Output(help.text);
	}

	[Command(name = "commands", description = "show commands")]
	static public void ShowCommands()
	{
		var commands = commands_
			.Select(x => string.Format(
				"- <b><i><color=#88ff88ff>{0}</color></i></b>\n" +
				"{1}",
				x.command,
				x.description))
			.Aggregate((str, x) => str + "\n" + x);
		Log.Output(commands);
	}

	static public CommandInfo[] GetAll()
	{
		return commands_ ?? (commands_ = System.AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(asm => asm.GetTypes())
			.SelectMany(type => type
				.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.SelectMany(method => method
					.GetCustomAttributes(typeof(CommandAttribute), false)
					.Cast<CommandAttribute>()
					.Select(attr => new CommandInfo(
						type.FullName,
						method.Name,
						attr.description,
						attr.name,
						method))))
			.OrderByDescending(x => x.command.Length)
			.ToArray());
	}

	internal class ParseBlockResult
	{
		public List<string> matches = new List<string>();
		public string input;
		public string output;
		public string pattern;
		public string placeholder;

		public ParseBlockResult(
			string input,
			string pattern,
			string placeholder)
		{
			this.input = this.output = input;
			this.pattern = pattern;
			this.placeholder = placeholder;
		}
	}

	static private ParseBlockResult ConvertBlockToPlaceholder(
		string input,
		string pattern,
		string placeholder)
	{
		var result = new ParseBlockResult(input, pattern, placeholder);

		var regex = new Regex(pattern);
		var n = 0;
		for (var m = regex.Match(input); m.Success; m = m.NextMatch(), ++n) {
			result.matches.Add(m.Value);
			result.output = regex.Replace(result.output, string.Format(placeholder, n), 1);
		}

		return result;
	}

	static private string ConvertPlaceholderToBlock(
		string input,
		ParseBlockResult result)
	{
		for (int i = 0; i < result.matches.Count; ++i) {
			var placeholder = string.Format(result.placeholder, i);
			input = input.Replace(placeholder, result.matches[i]);
		}

		return input;
	}

	static public string ConvertIntoCodeIfCommand(string code)
	{
		// NOTE: If two or more commands that have a same name are registered,
		//       the first one will be used here. It is not correct but works well because
		//       evaluation will be done after expanding the command to the code.

		// To consider commands with spaces, check if the head of the given code is consistent
		// with any command name. command list is ranked in descending order of the command string length.
		var commandInfo = Commands.GetAll().FirstOrDefault(
			x => (code.Length >= x.command.Length) &&
			     (code.Substring(0, x.command.Length) == x.command));

		// There is no command:
		if (commandInfo == null) {
			return code;
		}

		// Remove last semicolon.
		code = code.TrimEnd(';');

		// Check command format
		if (commandInfo.HasArguments()) {
			if (code.Substring(0, commandInfo.command.Length) != commandInfo.command) {
				return code;
			}
		} else {
			if (code.Length > commandInfo.command.Length) {
				return code;
			}
		}

		// Remove command and get only arguments.
		code = code.Substring(commandInfo.command.Length);

		// Store parentheses.
		var parentheses = ConvertBlockToPlaceholder(code, "\\([^\\)]+\\)", "<%paren{0}%>");
		code = parentheses.output;

		// Store quatation blocks.
		var quates = ConvertBlockToPlaceholder(code, "\"[^\"(\\\")]+\"", "<%quate{0}%>");
		code = quates.output;

		// Split arguments with space.
		var args = code.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);

		// Convert the command into the code.
		code = commandInfo.GetFormat(args);

		// Replace temporary quates placeholders to actual expressions.
		code = ConvertPlaceholderToBlock(code, quates);

		// Replace temporary parentheses placeholders to actual expressions.
		code = ConvertPlaceholderToBlock(code, parentheses);

		return code;
	}
}

}
