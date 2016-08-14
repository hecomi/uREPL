using UnityEngine;

namespace uREPL
{

[RequireComponent(typeof(Window))]
public class Main : MonoBehaviour
{
	public Parameters parameters;
	public EditorParameters editor;

	void Awake()
	{
		Evaluator.Initialize();
		GetComponent<Window>().main = this;
		UpdateAllDefaultCompletionPlugins();
	}

	void Update()
	{
		UpdateAllDefaultCompletionPlugins();
	}

	void UpdateAllDefaultCompletionPlugins()
	{
		UpdateCompletionPlugin<MonoCompletion>(parameters.useMonoCompletion);
		UpdateCompletionPlugin<CommandCompletion>(parameters.useCommandCompletion);
		UpdateCompletionPlugin<GameObjectNameCompletion>(parameters.useGameObjectNameCompletion);
		UpdateCompletionPlugin<GameObjectPathCompletion>(parameters.useGameObjectPathCompletion);
	}

	private void UpdateCompletionPlugin<T>(bool enabled) where T : MonoBehaviour
	{
		(GetComponent<T>() ?? gameObject.AddComponent<T>()).enabled = enabled; 
	}
}

}