using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
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

public enum CompletionType
{
	Mono    = 0,
	Command = 1,
	Path    = 2,
}

public struct CompletionInfo
{
	public CompletionType type;
	public string prefix;
	public string code;

	public CompletionInfo(CompletionType type, string prefix, string code)
	{
		this.type = type;
		this.prefix = prefix;
		this.code = code;
	}
}

public static class Core
{
	static private CommandInfo[] commands;
	static private string[] allGameObjectPaths;

	[RuntimeInitializeOnLoadMethod]
	static public void Initialize()
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

		Evaluator.Run("using UnityEngine;");
		// #if UNITY_EDITOR
		// Evaluator.Run("using UnityEditor;");
		// #endif

		// set commands.
		commands = Attributes.GetAllCommands();

		// set gameobject paths.
		UpdateAllGameObjectPaths();
	}

	static public void UpdateAllGameObjectPaths()
	{
		allGameObjectPaths = Utility.GetAllGameObjectPaths();
	}

	static public CompileResult Evaluate(string code)
	{
		var result = new CompileResult();
		result.code = code;

		// find commands at first and if found, expand it.
		var command = commands.FirstOrDefault(x => x.command == code.Replace(";", ""));
		if (command != null) {
			code = string.Format("{0}.{1}();", command.className, command.methodName);
		}

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

	static private string[] GetMonoCompletions(string input, out string prefix)
	{
		int i1, i2;

		// support generic type completion.
		i1 = input.LastIndexOf("<");
		i2 = input.LastIndexOf(">");
		if (i1 > i2 && i2 < input.Length) {
			input = input.Substring(i1 + 1);
			return Evaluator.GetCompletions(input, out prefix);
		}

		// support completion inner parenthesis
		i1 = input.LastIndexOf("(");
		i2 = input.LastIndexOf(")");
		if (i1 > i2 && i2 < input.Length) {
			input = input.Substring(i1 + 1);
			return Evaluator.GetCompletions(input, out prefix);
		}

		// otherwise
		return Evaluator.GetCompletions(input, out prefix);
	}

	static public CompletionInfo[] GetCompletions(string input, out string prefix)
	{
		// get context-based completions using Mono.
		var completions = GetMonoCompletions(input, out prefix);
		var _prefix = prefix;
		var monoCompletions = (completions == null) ?
			new List<CompletionInfo>() :
			completions
				.Select(x => new CompletionInfo(CompletionType.Mono, _prefix, x))
				.ToList();

		// get functions with a command attribute.
		var commandCompletions = (commands == null) ?
			new List<CompletionInfo>() :
			commands
				.Where(x => x.command.IndexOf(input) == 0)
				.Select(x => new CompletionInfo(CompletionType.Command, _prefix, x.command.Replace(_prefix, "")))
				.ToList();

		// get gameobjects paths
		var pathCompletions = new List<CompletionInfo>();
		var i1 = input.LastIndexOf("\"/");
		var i2 = input.LastIndexOf("\"");
		if (i1 != -1 && i1 == i2) {
			var partialPath = input.Substring(i1 + 1);
			pathCompletions = allGameObjectPaths
				.Where(x => x.IndexOf(partialPath) == 0)
				.Select(x => new CompletionInfo(CompletionType.Path, partialPath, x.Replace(partialPath, "")))
				.ToList();
		}

		return monoCompletions.Concat(commandCompletions).Concat(pathCompletions)
			.OrderBy(x => x.code).ToArray();
	}
}

}
