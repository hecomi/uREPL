using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityShell
{

static public class Log
{
	public enum Level
	{
		Verbose,
		Warn,
		Error
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
		var fileName = frame.GetFileName().Replace(Application.dataPath + "/", "");
		var line = frame.GetFileLineNumber();
		var meta = string.Format("{0}.{1} ({2}:{3})",
			method.DeclaringType.FullName,
			method.Name,
			fileName,
			line);
#else
		var meta = string.Format("{0}.{1}",
			method.DeclaringType.FullName,
			method.Name);
#endif
		Gui.instance.OutputLog(log, meta, level);
	}
}

}
