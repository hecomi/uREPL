using UnityEngine;
using System.Reflection;
using System.Linq;

namespace uREPL
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

	static public void Inspect(this GameObject gameObject)
	{
		Inspector.Inspect(gameObject);
	}

	static public void Inspect(this Component component)
	{
		Inspector.Inspect(component);
	}
}

}
