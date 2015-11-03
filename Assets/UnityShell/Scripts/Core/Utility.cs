using UnityEngine;
using System.Linq;

namespace UnityShell
{

static public class Utility
{
	static public string GetPath(this Transform tform)
	{
		if (tform.parent == null) {
			return "/" + tform.name;
		}
		return tform.parent.GetPath() + "/" + tform.name;
	}

	static public GameObject[] GetAllGameObjects()
	{
		return GameObject.FindObjectsOfType<GameObject>();
	}

	static public string[] GetAllGameObjectPaths()
	{
		return GetAllGameObjects().Select(go => go.transform.GetPath()).ToArray();
	}
}

}
