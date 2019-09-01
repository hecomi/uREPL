using UnityEngine;

namespace uREPL
{

public class EnterButton : MonoBehaviour 
{
    [SerializeField]
    Window window;

    void Start()
    {
        if (!window) {
            window = GetComponentInParent<Window>();
        }
    }

    public void Submit()
    {
        if (window) {
            Debug.Log(window.inputField.text);
            window.Submit(window.inputField.text, false);
        }
    }
}

}