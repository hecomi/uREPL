using UnityEngine;
using System.Collections;

namespace UnityShell
{

public static class Log
{
	static public void Output(string log)
	{
		UnityEngine.Debug.Log(log);
	}

	static public void Warn(string log)
	{
		UnityEngine.Debug.LogWarning(log);
	}

	static public void Error(string log)
	{
		UnityEngine.Debug.LogError(log);
	}
}

}
