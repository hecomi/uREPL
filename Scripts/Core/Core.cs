using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.CSharp;

namespace uREPL
{

public class CompileResult
{
	public enum Type { Success, Error, Partial }

	public Type type = Type.Success;
	public string code  = null;
	public string error = null;
	public object value = null;
}

public static class Core
{
	static private bool isInitialized = false;

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

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/uREPL")]
	[UnityEditor.MenuItem("GameObject/Create Other/uREPL")]
	static public void Create()
	{
		var prefab = Resources.Load("uREPL/Prefabs/uREPL");
		var instance = MonoBehaviour.Instantiate(prefab);
		instance.name = "uREPL";
	}
#endif

	static public void Initialize()
	{
		if (isInitialized) return;
		isInitialized = true;

		ReferenceAllAssemblies();
		SetUsings();

		Log.Initialize();
		Inspector.Initialize();
	}

	static private void ReferenceAllAssemblies()
	{
		// See the detailed information about this hack at:
		//   http://forum.unity3d.com/threads/mono-csharp-evaluator.102162/
		for (int n = 0; n < 2;) {
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				if (assembly == null) continue;
				Evaluator.ReferenceAssembly(assembly);
			}
			Evaluator.Evaluate("null;");
			n++;
		}
	}

	static private void SetUsings()
	{
		Evaluator.Run("using uREPL;");
		Evaluator.Run("using UnityEngine;");
		// #if UNITY_EDITOR
		// Evaluator.Run("using UnityEditor;");
		// #endif
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


	static private string ConvertIntoCodeIfCommand(string code)
	{
		// NOTE: If two or more commands that have a same name are registered,
		//       the first one will be used here. It is not correct but works well because
		//       evaluation will be done after expanding the command to the code.

		// To consider commands with spaces, check if the head of the given code is consistent
		// with any command name. command list is ranked in descending order of the command string length.
		var commandInfo = Commands.GetAll().FirstOrDefault(
			x => (code.Length >= x.command.Length) && 
			     (code.Substring(0, x.command.Length) == x.command));
		if (commandInfo == null) {
			return code;
		}

		// Remove last semicolon.
		code = code.TrimEnd(';');

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
		code  = string.Format("{0}.{1}(", commandInfo.className, commandInfo.methodName);
		code += string.Join(", ", args);
		code += ");";

		// Replace temporary quates placeholders to actual expressions.
		code = ConvertPlaceholderToBlock(code, quates);

		// Replace temporary parentheses placeholders to actual expressions.
		code = ConvertPlaceholderToBlock(code, parentheses);

		return code;
	}

	static public CompileResult Evaluate(string code)
	{
		var result = new CompileResult();
		result.code = code;

		// find commands at first and expand it if found.
		code = ConvertIntoCodeIfCommand(code);

		// if not match, eval the code using Mono.
		object ret = null;
		bool hasReturnValue = false;

		var originalOutput = Evaluator.MessageOutput;
		var errorWriter = new System.IO.StringWriter();
		bool isPartial = false;
		Evaluator.MessageOutput = errorWriter;
		try {
			isPartial = Evaluator.Evaluate(code, out ret, out hasReturnValue) != null;
		} catch (System.Exception e) {
			errorWriter.Write(e.Message);
		}
		Evaluator.MessageOutput = originalOutput;

		var error = errorWriter.ToString();
		if (!string.IsNullOrEmpty(error)) {
			error = error.Replace("{interactive}", "");
			var lastLineBreakPos = error.LastIndexOf('\n');
			if (lastLineBreakPos != -1) {
				error = error.Remove(lastLineBreakPos);
			}
			result.type  = CompileResult.Type.Error;
			result.error = error;
			return result;
		}
		errorWriter.Dispose();

		if (isPartial) {
			result.type = CompileResult.Type.Partial;
			return result;
		}

		result.type = CompileResult.Type.Success;
		result.value = (hasReturnValue && ret != null) ? ret : "null";
		return result;
	}

	static public string GetVars()
	{
		return Evaluator.GetVars();
	}

	static public string GetUsing()
	{
		return Evaluator.GetUsing();
	}

	[Command(name = "show vars", description = "Show all local variables")]
	static public void ShowVars()
	{
		Log.Output(GetVars());
	}

	[Command(name = "show using", description = "Show all using")]
	static public void ShowUsing()
	{
		Log.Output(GetUsing());
	}

	[Command(name = "test", description = "Show all using")]
	static public void Test(string hoge, float fuga)
	{
		Debug.Log(hoge + " " + fuga);
	}
}

}
