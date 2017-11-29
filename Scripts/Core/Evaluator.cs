using UnityEngine;

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

		Mono.Initialize();
		SetUsings();

		Log.Initialize();
		Inspector.Initialize();

		Evaluate("null;");
	}

	static private void SetUsings()
	{
		Mono.Run("using uREPL;");
		Mono.Run("using System;");
		Mono.Run("using UnityEngine;");
		// #if UNITY_EDITOR
		// Mono.Run("using UnityEditor;");
		// #endif
	}

	static public CompileResult Evaluate(string code, bool autoCompleteComma = true)
	{
		if (autoCompleteComma && !code.EndsWith(";")) {
			code += ";";
		}

		var result = new CompileResult();
		result.code = code;

		// find commands and expand it if found.
        if (RuntimeCommands.ConvertIntoCodeIfCommand(ref code) || 
            Commands.ConvertIntoCodeIfCommand(ref code)) {
            // the give code is a command and converted into an actual code.
        }

		// eval the code using Mono.
		object ret = null;
		bool hasReturnValue = false;
		bool isPartial = false;
		try {
			isPartial = Mono.Evaluate(code, out ret, out hasReturnValue) != null;
		} catch (System.Exception e) {
			UnityEngine.Debug.LogError(e.Message);
		}

		var error = Mono.lastOutput;
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
        return Mono.GetCompletions(input, out prefix);
	}

	static public string GetVars()
	{
		return Mono.GetVars();
	}

	static public string GetUsing()
	{
		return Mono.GetUsing();
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
