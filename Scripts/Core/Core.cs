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

public static class Core
{
	static private bool isInitialized = false;
	static private List<CompletionPlugin> completionPlugins = new List<CompletionPlugin>();

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/uREPL")]
	static public void Create()
	{
		var instance = MonoBehaviour.Instantiate(Resources.Load("uREPL"));
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

	static public CompileResult Evaluate(string code)
	{
		var result = new CompileResult();
		result.code = code;

		// find commands at first and if found, expand it.
		var commands = Commands.GetAll();
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

	static public CompletionInfo[] GetCompletions(string input)
	{
		var result = new CompletionInfo[] {};

		foreach (var plugin in completionPlugins) {
			var completions = plugin.GetCompletions(input);
			if (completions != null) {
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

	[Command(command = "show vars", description = "Show all local variables")]
	static public void ShowVars()
	{
		Debug.Log(GetVars());
		Log.Output(GetVars());
	}

	[Command(command = "show using", description = "Show all using")]
	static public void ShowUsing()
	{
		Log.Output(GetUsing());
	}
}

}
