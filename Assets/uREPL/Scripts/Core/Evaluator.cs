using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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

public static class Evaluator
{
	static private bool isInitialized = false;

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
				Mono.CSharp.Evaluator.ReferenceAssembly(assembly);
			}
			Mono.CSharp.Evaluator.Evaluate("null;");
			n++;
		}
	}

	static private void SetUsings()
	{
		Mono.CSharp.Evaluator.Run("using uREPL;");
		Mono.CSharp.Evaluator.Run("using UnityEngine;");
		// #if UNITY_EDITOR
		// Mono.CSharp.Evaluator.Run("using UnityEditor;");
		// #endif
	}

	static public CompileResult Evaluate(string code, bool autoCompleteComma = true)
	{
		if (autoCompleteComma && !code.EndsWith(";")) {
			code += ";";
		}

		var result = new CompileResult();
		result.code = code;

		// find commands at first and expand it if found.
		code = Commands.ConvertIntoCodeIfCommand(code);

		// if not match, eval the code using Mono.
		object ret = null;
		bool hasReturnValue = false;

		var originalOutput = Mono.CSharp.Evaluator.MessageOutput;
		var errorWriter = new System.IO.StringWriter();
		bool isPartial = false;
		Mono.CSharp.Evaluator.MessageOutput = errorWriter;
		try {
			isPartial = Mono.CSharp.Evaluator.Evaluate(code, out ret, out hasReturnValue) != null;
		} catch (System.Exception e) {
			errorWriter.Write(e.Message);
		}
		Mono.CSharp.Evaluator.MessageOutput = originalOutput;

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

	static public string[] GetCompletions(string input, out string prefix)
	{
        return Mono.CSharp.Evaluator.GetCompletions(input, out prefix);
	}

	static public string GetVars()
	{
		return Mono.CSharp.Evaluator.GetVars();
	}

	static public string GetUsing()
	{
		return Mono.CSharp.Evaluator.GetUsing();
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
