using UnityEngine;

namespace uREPL
{

[RequireComponent(typeof(Window))]
public class Main : MonoBehaviour
{
	public Parameters parameters;

#if UNITY_EDITOR
	public EditorParameters editor;
#endif

	void Awake()
	{
		GetComponent<Window>().main = this;
	}

	void Start()
	{
		Evaluator.Initialize();
		AddDefaultCompletionPlugins();
	}

	void AddDefaultCompletionPlugins()
	{
		if (parameters.useMonoCompletion)           gameObject.AddComponent<MonoCompletion>();
		if (parameters.useCommandCompletion)        gameObject.AddComponent<CommandCompletion>();
		if (parameters.useGameObjectNameCompletion) gameObject.AddComponent<GameObjectNameCompletion>();
		if (parameters.useGameObjectPathCompletion) gameObject.AddComponent<GameObjectPathCompletion>();
	}
}

}