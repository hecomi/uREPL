using UnityEngine;

namespace uREPL
{

[System.Serializable]
public class Parameters
{
	#region [Completion Plugin]
	public bool useMonoCompletion           = true;
	public bool useCommandCompletion        = true;
	public bool useGameObjectNameCompletion = true;
	public bool useGameObjectPathCompletion = true;
	#endregion

	#region [Parameters]
	public float completionDelay = 0f;
	public float annotationDelay = 0.5f;
	#endregion
}

[System.Serializable]
public class EditorParameters
{
	public bool completionFoldOut = true;
	public bool parametersFoldOut = true;
}

}
