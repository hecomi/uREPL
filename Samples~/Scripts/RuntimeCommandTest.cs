using UnityEngine;

public class RuntimeCommandTest : MonoBehaviour
{
    void OnEnable()
    {
        uREPL.RuntimeCommands.Register("func1", () => Debug.Log("hoge"), "output hoge");
        uREPL.RuntimeCommands.Register("func2", x => Debug.Log(x), "outout given argument");
        uREPL.RuntimeCommands.Register("func3", (x, y) => Debug.Log((int)x * (int)y), "multiply arg0 by arg1");
        uREPL.RuntimeCommands.Register("func4", (x, y, z) => Debug.Log(x + " " + y + " " + z), "output all arguments");
        uREPL.RuntimeCommands.Register("func5", (int x) => Debug.Log(x), "output int value");
        uREPL.RuntimeCommands.Register("func6", (string x, float y) => Debug.Log(x + " is " + y), "output arg0 is arg1");
        uREPL.RuntimeCommands.Register("func7", (Vector3 pos, Quaternion rot, Vector3 scale) => Debug.Log(pos + " " + rot + " " + scale), "output unity-type values.");
    }

    void OnDisable()
    {
        uREPL.RuntimeCommands.Unregister("func1");
        uREPL.RuntimeCommands.Unregister("func2");
        uREPL.RuntimeCommands.Unregister("func3");
        uREPL.RuntimeCommands.Unregister("func4");
        uREPL.RuntimeCommands.Unregister("func5");
        uREPL.RuntimeCommands.Unregister("func6");
        uREPL.RuntimeCommands.Unregister("func7");
    }
}
