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
	private const string unityEngineClassXmlPath = "uREPL/Xmls/UnityEngineClasses";

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
		var xml = Resources.Load(unityEngineClassXmlPath) as TextAsset;
		var serializer = new XmlSerializer(typeof(TypeDataCollection));
		using (var reader = new StringReader(xml.text)) {
			types_ = serializer.Deserialize(reader) as TypeDataCollection;
		}

		base.Awake();
	}

	public int GetPosIfInsideBracket(string input, string left, string right)
	{
		var i1 = input.LastIndexOf(left);
		var i2 = input.LastIndexOf(right);
		return (i1 > i2 && i2 < input.Length) ? (i1 + 1) : -1;
	}

	public bool IsInsideQuotation(string input, string quot = "\"")
	{
		var cnt = 0;
		var pos = -1;
		for (;;) {
			pos = input.IndexOf(quot, pos + 1);
			if (pos == -1) break;
			++cnt;
		}
		return cnt % 2 == 1;
	}

	public override CompletionInfo[] GetCompletions(string input)
	{
		bool isComplemented = false;
		string[] result = null;
		string prefix = "";
		int index = 0;

		// skip if inside quotation
		if (IsInsideQuotation(input)) {
			return null;
		}

		// split by '=, \s'
		var inputParts = input.Split(new char[] { '=', ' ', '	' });
		input = inputParts.Last();

		// completion inner block
		index = GetPosIfInsideBracket(input, "{", "}");
		if (!isComplemented && index != -1) {
			input = input.Substring(index);
		}

		// generic type completion
		index = GetPosIfInsideBracket(input, "<", ">");
		if (!isComplemented && index != -1) {
			input = input.Substring(index);
			result = Evaluator.GetCompletions(input, out prefix);
			isComplemented = true;
		}

		// completion inner parenthesis
		index = GetPosIfInsideBracket(input, "(", ")");
		if (!isComplemented && index != -1) {
			input = input.Substring(index);
			result = Evaluator.GetCompletions(input, out prefix);
			isComplemented = true;
		}

		// otherwise
		if (!isComplemented) {
			result = Evaluator.GetCompletions(input, out prefix);
			isComplemented = true;
		}

		return (result == null) ? null : result
			.Select(completion => new CompletionInfo(
				prefix,
				prefix + completion,
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
