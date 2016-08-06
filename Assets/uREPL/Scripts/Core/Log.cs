using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace uREPL
{

static public class Log
{
	public enum Level
	{
		Verbose,
		Warn,
		Error
	}

	public struct Data
	{
		public string log;
		public string meta;
		public Level level;

		public Data(string log, string meta, Level level)
		{
			this.log = log;
			this.meta = meta;
			this.level = level;
		}
	}

#if UNITY_EDITOR
	static private string dataPath = "";
#endif
	static private bool isInitialized = false;

	static public void Initialize()
	{
		if (isInitialized) return;
		isInitialized = true;

#if UNITY_EDITOR
		dataPath = Application.dataPath;
#endif
	}

	static public void Output(string log)
	{
		Output(log, Level.Verbose, new StackFrame(1, true));
	}

	static public void Warn(string log)
	{
		Output(log, Level.Warn, new StackFrame(1, true));
	}

	static public void Error(string log)
	{
		Output(log, Level.Error, new StackFrame(1, true));
	}

	static public void Output(string log, Level level, StackFrame frame)
	{
		var method = frame.GetMethod();
#if UNITY_EDITOR
		var fileName = frame.GetFileName();
		if (!string.IsNullOrEmpty(fileName)) {
			fileName = fileName.Replace(dataPath + "/", "");
		}
		var line = frame.GetFileLineNumber();
		var meta = string.Format("{0}.{1}() ({2}:{3})",
			method.DeclaringType.FullName,
			method.Name,
			fileName ?? "$",
			line);
#else
		var meta = string.Format("{0}.{1}",
			method.DeclaringType.FullName,
			method.Name);
#endif
		Window.selected.OutputLog(new Data(log, meta, level));
	}
}

}
