using UnityEngine;
using System.Collections;
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

	static public void Inspect<T>(this T component) where T : Component
	{
		Inspector.Inspect(component);
	}

	static public void RunOnEndOfFrame(System.Action func)
	{
		if (!Window.selected) return;
		Window.selected.StartCoroutine(_RunOnEndOfFrame(func));
	}

	static private IEnumerator _RunOnEndOfFrame(System.Action func)
	{
		yield return new WaitForEndOfFrame();
		func();
	}

	static public void RunOnNextFrame(System.Action func)
	{
		if (!Window.selected) return;
		Window.selected.StartCoroutine(_RunOnNextFrame(func));
	}

	static private IEnumerator _RunOnNextFrame(System.Action func)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		func();
	}
}

}
