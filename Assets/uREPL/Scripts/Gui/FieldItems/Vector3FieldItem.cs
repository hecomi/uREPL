using UnityEngine.UI;
using System;

namespace uREPL
{

public class Vector3FieldItem : FieldItem
{
    public InputField xInputField;
    public InputField yInputField;
    public InputField zInputField;

    public override object value
    {
        get
        {
            return Activator.CreateInstance(
                fieldType,
                xInputField.text.AsFloat(),
                yInputField.text.AsFloat(),
                zInputField.text.AsFloat());
        }
        protected set
        {
            xInputField.text = fieldType.GetField("x").GetValue(value).AsString();
            yInputField.text = fieldType.GetField("y").GetValue(value).AsString();
            zInputField.text = fieldType.GetField("z").GetValue(value).AsString();
        }
    }

    void Start()
    {
        xInputField.onEndEdit.AddListener(OnSubmit);
        yInputField.onEndEdit.AddListener(OnSubmit);
        zInputField.onEndEdit.AddListener(OnSubmit);
    }

    void OnDestroy()
    {
        xInputField.onEndEdit.RemoveListener(OnSubmit);
        yInputField.onEndEdit.RemoveListener(OnSubmit);
        zInputField.onEndEdit.RemoveListener(OnSubmit);
    }

    void Update()
    {
        if (!xInputField.isFocused &&
            !yInputField.isFocused &&
            !zInputField.isFocused) {
            value = componentType.GetField(fieldName).GetValue(component);
        }
    }

    void OnSubmit(string text)
    {
        componentType.GetField(fieldName).SetValue(component, value);
    }
}

}
