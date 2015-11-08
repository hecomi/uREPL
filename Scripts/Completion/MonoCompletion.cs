using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using Mono.CSharp;

namespace uREPL
{

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

	protected override void Awake()
	{
		var xml = Resources.Load("UnityEngineClasses") as TextAsset;
		var serializer = new XmlSerializer(typeof(TypeDataCollection));
		using (var reader = new StringReader(xml.text)) {
			types_ = serializer.Deserialize(reader) as TypeDataCollection;
		}

		base.Awake();
	}

	public override CompletionInfo[] GetCompletions(string input)
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

}
