using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

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

	protected override void OnEnable()
	{
		var xml = Resources.Load(unityEngineClassXmlPath) as TextAsset;
		var serializer = new XmlSerializer(typeof(TypeDataCollection));
		using (var reader = new StringReader(xml.text)) {
			types_ = serializer.Deserialize(reader) as TypeDataCollection;
		}

		base.OnEnable();
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

	private CompletionInfo[] ConvertToCompletionInfoArray(string[] result, string prefix)
	{
		return (result == null) ? null : result
			.Select(completion => new CompletionInfo(
				prefix,
				prefix + completion,
				"M",
				new Color32(50, 70, 240, 255)))
			.Select(completion => {
				var type = types_.list.FirstOrDefault(x => x.name == completion.code);
				if (type != null) {
					completion.description = type.description;
				}
				return completion;
			})
			.ToArray();
	}

	private CompletionInfo[] GetCustomCompletions(string input, string codeAddedPreviously = "")
	{
		string[] result = null;
		string prefix = "";
		bool isComplemented = false;
		int index = 0;

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
			result = Evaluator.GetCompletions(codeAddedPreviously + input, out prefix);
			isComplemented = true;
		}

		// completion inner parenthesis
		index = GetPosIfInsideBracket(input, "(", ")");
		if (!isComplemented && index != -1) {
			input = input.Substring(index);
			result = Evaluator.GetCompletions(codeAddedPreviously + input, out prefix);
			isComplemented = true;
		}

		// completion inner parenthesis (2)
		index = GetPosIfInsideBracket(input, "[", "]");
		if (!isComplemented && index != -1) {
			input = input.Substring(index);
			result = Evaluator.GetCompletions(codeAddedPreviously + input, out prefix);
			isComplemented = true;
		}

		// otherwise
		if (!isComplemented) {
			result = Evaluator.GetCompletions(codeAddedPreviously + input, out prefix);
			isComplemented = true;
		}

		return ConvertToCompletionInfoArray(result, prefix);
	}

	public override CompletionInfo[] GetCompletions(string input)
	{
		// skip if inside quotation
		if (IsInsideQuotation(input)) {
			return null;
		}

		// At first, try to complement the non-edited input.
		// NOTE: this cause crash when input matches some patterns (e.g. "for (")
		/*
		string prefix;
		var result = Evaluator.GetCompletions(input, out prefix);
		if (result != null) {
			return ConvertToCompletionInfoArray(result, prefix);
		}
		*/

		// Next, try to complement input whose last line is edited.
		// (If there is only on line, skip it.)
		var lines = input.Split(new char[] { '\n' });
		if (lines.Length == 1) {
			return GetCustomCompletions(input);
		}

		var lastLine = lines.Last();
		System.Array.Clear(lines, lines.Length - 1, 1);
		var linesExceptForLast = string.Join("", lines);

		var completions = GetCustomCompletions(lastLine, linesExceptForLast);
		if (completions != null) {
			return completions;
		}

		// At last, try to complete only last line
		return GetCustomCompletions(lastLine);
	}
}

}
