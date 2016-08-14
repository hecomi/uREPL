using UnityEngine;

namespace uREPL
{

[System.Serializable]
public class Parameters
{
	public bool useMonoCompletion           = true;
	public bool useCommandCompletion        = true;
	public bool useGameObjectNameCompletion = true;
	public bool useGameObjectPathCompletion = true;
}

#if UNITY_EDITOR
[System.Serializable]
public class EditorParameters
{
	public bool completionFoldOut = false;
}
#endif

}
