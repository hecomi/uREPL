using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

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
	public string command;

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
	public ParameterInfo[] parameters;

	public CommandInfo(string className, string methodName, string description, string command, ParameterInfo[] parameters)
	{
		this.className   = className;
		this.methodName  = methodName;
		this.description = description;
		this.command     = string.IsNullOrEmpty(command) ? methodName : command;
		this.parameters  = parameters;
	}
}

public static class Commands
{
	private const string helpTextFile = "uREPL/Xmls/uReplHelp";
	static private CommandInfo[] commands_;

	[Command(command = "help", description = "show help")]
	static public void ShowHelp()
	{
		var help = Resources.Load(helpTextFile) as TextAsset;
		Log.Output(help.text);
	}

	[Command(command = "commands", description = "show commands")]
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
						string.Format("{0} <color=#888888ff>(<i>{1}.{2}()</i>)</color>",
							attr.description,
							type.FullName,
							method.Name),
						attr.command,
						method.GetParameters()))))
			.ToArray());
	}
}

}
