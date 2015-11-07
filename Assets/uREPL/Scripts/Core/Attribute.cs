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

    public CommandAttribute(string description)
	{
		this.description = description;
	}
}

public class CommandInfo
{
	public string methodName;
	public string className;
	public string description;
	public string command;

	public CommandInfo(string className, string methodName, string description, string command)
	{
		this.className   = className;
		this.methodName  = methodName;
		this.description = description;
		this.command     = string.IsNullOrEmpty(command) ? methodName : command;
	}
}

public static class Attributes
{
	static public CommandInfo[] GetAllCommands()
	{
		return System.AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(asm => asm.GetTypes())
			.SelectMany(type => type
				.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.SelectMany(method => method
					.GetCustomAttributes(typeof(CommandAttribute), false)
					.Cast<CommandAttribute>()
					.Select(attr => new CommandInfo(
						type.FullName,
						method.Name,
						string.Format("{0} <color=#888888ff>({1}.{2})</color>",
							attr.description,
							type.FullName,
							method.Name),
						attr.command))))
			.ToArray();
	}
}

}
