using UnityEngine;
using System;
using System.Collections.Generic;
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

	protected virtual void Awake()
	{
		Core.RegisterCompletionPlugins(this);
	}

	protected virtual void OnDestroy()
	{
		Core.UnregisterCompletionPlugins(this);
	}
}

}
