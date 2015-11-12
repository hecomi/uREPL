using UnityEngine;
using System.Linq;

namespace uREPL
{

public class GameObjectNameCompletion : CompletionPlugin
{
	static private string[] allGameObjectNames_;

	protected override void Awake()
	{
		allGameObjectNames_ = GameObject.FindObjectsOfType<GameObject>()
			.Select(go => go.name)
			.ToArray();

		base.Awake();
	}

	public bool IsInsideQuotation(string input, string quot = "\"")
	{
		var cnt = 0;
		var pos = -1;
		for (;;) {
			pos = input.IndexOf(quot, pos + 1);
			if (pos == -1) break;
			++cnt;
		}
		return cnt % 2 == 1;
	}

	public override CompletionInfo[] GetCompletions(string input)
	{
		if (!IsInsideQuotation(input)) return null;

		var partialName = input.Substring(input.LastIndexOf("\"") + 1);
		return allGameObjectNames_
			.Where(name => name.IndexOf(partialName) != -1)
			.Select(name => new CompletionInfo(
				partialName,
				name,
				"G",
				new Color32(30, 200, 80, 255)))
			.ToArray();
	}
}

}
