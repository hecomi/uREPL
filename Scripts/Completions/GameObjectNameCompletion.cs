using UnityEngine;
using System.Linq;

namespace uREPL
{

public class GameObjectNameCompletion : CompletionPlugin
{
	private struct GameObjectInfo
	{
		public string name;
		public string path;
		public GameObjectInfo(string name, string path)
		{
			this.name = name;
			this.path = path;
		}
	}

	static private GameObjectInfo[] allGameObjects_;

	protected override void OnEnable()
	{
		allGameObjects_ = GameObject.FindObjectsOfType<GameObject>()
			.Select(go => new GameObjectInfo(
				go.name,
				go.transform.GetPath()))
			.ToArray();

		base.OnEnable();
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
		return allGameObjects_
			.Where(info => info.name.IndexOf(partialName) == 0)
			.Select(info => new CompletionInfo(
				partialName,
				info.name,
				"G",
				new Color32(30, 200, 80, 255),
				info.path))
			.ToArray();
	}
}

}
