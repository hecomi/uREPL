using UnityEngine;

namespace uREPL
{

[RequireComponent(typeof(Window))]
public class Main : MonoBehaviour
{
    public Parameters parameters;
    public EditorParameters editor;
    public string preLoadCode = "/* Write code here which you want to run just after start. */";

    void Awake()
    {
        Evaluator.Initialize();
        RunPreLoadCode();
        GetComponent<Window>().main = this;
        UpdateAllDefaultCompletionPlugins();
    }

    void Update()
    {
        UpdateAllDefaultCompletionPlugins();
    }

    void RunPreLoadCode()
    {
        Evaluator.Evaluate(preLoadCode, false);
    }

    void UpdateAllDefaultCompletionPlugins()
    {
        UpdateCompletionPlugin<MonoCompletion>(parameters.useMonoCompletion);
        UpdateCompletionPlugin<CommandCompletion>(parameters.useCommandCompletion);
        UpdateCompletionPlugin<RuntimeCommandCompletion>(parameters.useRuntimeCommandCompletion);
        UpdateCompletionPlugin<GameObjectNameCompletion>(parameters.useGameObjectNameCompletion);
        UpdateCompletionPlugin<GameObjectPathCompletion>(parameters.useGameObjectPathCompletion);
        UpdateCompletionPlugin<GlobalClassCompletion>(parameters.useGlobalClassCompletion);
    }

    private void UpdateCompletionPlugin<T>(bool enabled) where T : MonoBehaviour
    {
        (GetComponent<T>() ?? gameObject.AddComponent<T>()).enabled = enabled; 
    }
}

}