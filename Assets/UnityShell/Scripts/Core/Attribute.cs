using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace UnityShell
{

[AttributeUsage(
	AttributeTargets.Method,
	AllowMultiple = false,
	Inherited = false
)]
public sealed class CommandAttribute : Attribute
{
    private string description_;
    public string description {
		get { return description_; }
		private set { description_ = value; }
	}

    public CommandAttribute(string description)
	{
		this.description = description;
	}
}

public static class Attributes
{
	public struct CommandInfo
	{
		public string methodName;
		public string className;
		public string description;

		public CommandInfo(string className, string methodName, string description)
		{
			this.className   = className;
			this.methodName  = methodName;
			this.description = description;
		}
	}

	static public List<CommandInfo> GetAllCommands()
	{
		return System.AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(asm => asm.GetTypes())
			.SelectMany(type => type
				.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.SelectMany(method => method
					.GetCustomAttributes(typeof(CommandAttribute), false)
					.Cast<CommandAttribute>()
					.Select(attr => new CommandInfo(
						type.Name,
						method.Name,
						string.Format("{0} ({1}.{2})", attr.description, type.Name, method.Name)))))
			.ToList();
	}
}

}
