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
	private const string uReplPrefabPath = "uREPL/Prefabs/uREPL";
	static private bool isInitialized = false;
	static private List<CompletionPlugin> completionPlugins = new List<CompletionPlugin>();

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/uREPL")]
	[UnityEditor.MenuItem("GameObject/Create Other/uREPL")]
	static public void Create()
	{
		var prefab = Resources.Load(uReplPrefabPath);
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

		// setup log
		Log.Initialize();
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

	static public void RegisterCompletionPlugins(CompletionPlugin plugin)
	{
		completionPlugins.Add(plugin);
	}

	static public void UnregisterCompletionPlugins(CompletionPlugin plugin)
	{
		if (completionPlugins.Contains(plugin)) {
			completionPlugins.Remove(plugin);
		}
	}

	static private string ConvertIntoCodeIfCommand(string code)
	{
		// NOTE: If two or more commands that have a same name are registered,
		//       the first one will be used here. It is not correct but works well because
		//       evaluation will be done after expanding the command to the code.

		// To consider commands with spaces, check if the head of the given code is consistent
		// with any command name. command list is ranked in descending order of the command string length.
		var commandInfo = Commands.GetAll().FirstOrDefault(
			x => (code.Length >= x.command.Length) && (code.Substring(0, x.command.Length) == x.command));
		if (commandInfo == null) {
			return code;
		}

		var argsTmp = code.Substring(commandInfo.command.Length).TrimEnd(';').Split(
			new string[] { " ", "\t" },
			System.StringSplitOptions.None).ToList();

		// Check arguments with spaces inner semicolons (e.g. $ print "foo \"bar\"").
		var args = new List<string>();
		var semicolonCount = 0;
		for (int i = 0; i < argsTmp.Count; ++i) {
			bool isPartial = semicolonCount % 2 == 1;
			var arg = argsTmp[i];
			var regex = new Regex("(^|[^\\\\]+)\"");
			semicolonCount += regex.Matches(arg).Count;
			if (isPartial) {
				args[args.Count - 1] += " " + arg;
			} else if (!string.IsNullOrEmpty(arg)) {
				args.Add(arg);
			}
		}

		// Convert the command into the code.
		if (commandInfo != null) {
			code  = string.Format("{0}.{1}(", commandInfo.className, commandInfo.methodName);
			code += string.Join(", ", args.ToArray());
			code += ");";
		}

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

	static public CompletionInfo[] GetCompletions(string input)
	{
		var result = new CompletionInfo[] {};

		foreach (var plugin in completionPlugins) {
			var completions = plugin.GetCompletions(input);
			if (completions != null && completions.Length > 0) {
				result = result.Concat(completions).ToArray();
			}
		}

		return result.OrderBy(x => x.code).ToArray();
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
}

}
