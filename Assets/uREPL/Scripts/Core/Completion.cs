using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using Mono.CSharp;

namespace uREPL
{

public class CompletionInfo
{
	public string prefix;
	public string code;
	public string mark;
	public Color32 color;
	public string description;

	public CompletionInfo(
		string prefix,
		string code,
		string mark,
		Color32 color,
		string description = "")
	{
		this.prefix = prefix;
		this.code = code;
		this.mark = mark;
		this.color = color;
		this.description = description;
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
	[Serializable()]
	public class TypeData
	{
		[XmlElement("name")]
		public string name;
		[XmlElement("desc")]
		public string description;
	}

	[Serializable()]
	[XmlRoot("root")]
	public class TypeDataCollection
	{
		[XmlArray("types")]
		[XmlArrayItem("type", typeof(TypeData))]
		public TypeData[] list { get; set; }
	}

	private TypeDataCollection types_;

	public override void Initialize()
	{
		var xml = Resources.Load("UnityEngineClasses") as TextAsset;
		var serializer = new XmlSerializer(typeof(TypeDataCollection));
		using (var reader = new StringReader(xml.text)) {
			types_ = serializer.Deserialize(reader) as TypeDataCollection;
		}
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
			.Select(completion => {
				var type = types_.list.FirstOrDefault(x => x.name == (completion.prefix + completion.code));
				if (type != null) {
					completion.description = type.description;
				}
				return completion;
			})
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
		GetAllCommands();
	}

	private void GetAllCommands()
	{
		commands = System.AppDomain.CurrentDomain.GetAssemblies()
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
					new Color32(200, 50, 30, 255),
					x.description))
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
