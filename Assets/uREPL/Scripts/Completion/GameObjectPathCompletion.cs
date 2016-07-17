using UnityEngine;
using System.Linq;

namespace uREPL
{

public class GameObjectPathCompletion : CompletionPlugin
{
	static private string[] allGameObjectPaths;

	protected override void OnEnable()
	{
		allGameObjectPaths = Utility.GetAllGameObjectPaths();

		base.OnEnable();
	}

	public override CompletionInfo[] GetCompletions(string input)
	{
		var pathCompletions = new CompletionInfo[] {};
		var i1 = input.LastIndexOf("\"/");
		var i2 = input.LastIndexOf("\"");
		if (i1 != -1 && i1 == i2) {
			var partialPath = input.Substring(i1 + 1);
			pathCompletions = allGameObjectPaths
				.Where(path => path.IndexOf(partialPath) == 0)
				.Select(path => new CompletionInfo(
					partialPath,
					path,
					"G",
					new Color32(30, 200, 80, 255)))
				.ToArray();
		}
		return pathCompletions;
	}
}

}
