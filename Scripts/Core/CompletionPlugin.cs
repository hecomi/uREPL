using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CSharp;

namespace uREPL
{

public class CompletionInfo
{
	public string prefix;
	public string code;
	public string mark;
	public Color32 color;
	public string description;

	public CompletionInfo(
		string prefix,
		string code,
		string mark,
		Color32 color,
		string description = "")
	{
		this.prefix = prefix;
		this.code = code;
		this.mark = mark;
		this.color = color;
		this.description = description;
	}
}

public abstract class CompletionPlugin : MonoBehaviour
{
	public abstract CompletionInfo[] GetCompletions(string input);

	protected virtual void OnEnable()
	{
		CompletionPluginManager.RegisterCompletionPlugins(this);
	}

	protected virtual void OnDisable()
	{
		CompletionPluginManager.UnregisterCompletionPlugins(this);
	}
}

public class CompletionPluginManager
{
	static private List<CompletionPlugin> completionPlugins = new List<CompletionPlugin>();

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
}

}
