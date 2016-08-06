using UnityEngine;

namespace uREPL
{

[RequireComponent(typeof(Window))]
public class Main : MonoBehaviour
{
	[HeaderAttribute("Completion Methods")]
	public bool useMonoCompletion           = true;
	public bool useCommandCompletion        = true;
	public bool useGameObjectNameCompletion = true;
	public bool useGameObjectPathCompletion = true;

	void Start()
	{
		Evaluator.Initialize();
		AddDefaultCompletionPlugins();
	}

	void AddDefaultCompletionPlugins()
	{
		if (useMonoCompletion)           gameObject.AddComponent<MonoCompletion>();
		if (useCommandCompletion)        gameObject.AddComponent<CommandCompletion>();
		if (useGameObjectNameCompletion) gameObject.AddComponent<GameObjectNameCompletion>();
		if (useGameObjectPathCompletion) gameObject.AddComponent<GameObjectPathCompletion>();
	}
}

}