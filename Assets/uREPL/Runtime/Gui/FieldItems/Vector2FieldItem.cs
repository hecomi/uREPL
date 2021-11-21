using UnityEngine.UI;
using System;

namespace uREPL
{

public class Vector2FieldItem : FieldItem
{
    public InputField xInputField;
    public InputField yInputField;

    public override object value
    {
        get
        {
            return Activator.CreateInstance(
                fieldType,
                xInputField.text.AsFloat(),
                yInputField.text.AsFloat());
        }
        protected set
        {
            xInputField.text = fieldType.GetField("x").GetValue(value).AsString();
            yInputField.text = fieldType.GetField("y").GetValue(value).AsString();
        }
    }

    void Start()
    {
        xInputField.onEndEdit.AddListener(OnSubmit);
        yInputField.onEndEdit.AddListener(OnSubmit);
    }

    void OnDestroy()
    {
        xInputField.onEndEdit.RemoveListener(OnSubmit);
        yInputField.onEndEdit.RemoveListener(OnSubmit);
    }

    void Update()
    {
        if (!xInputField.isFocused && !yInputField.isFocused) {
            value = componentType.GetField(fieldName).GetValue(component);
        }
    }

    void OnSubmit(string text)
    {
        componentType.GetField(fieldName).SetValue(component, value);
    }
}

}
