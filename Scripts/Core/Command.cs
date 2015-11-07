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

public static class Commands
{
}

}
