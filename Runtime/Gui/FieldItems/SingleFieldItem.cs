using UnityEngine.UI;
using System;

namespace uREPL
{

public class SingleFieldItem : FieldItem
{
    public InputField valueInputField;

    public override object value
    {
        get { return Convert.ChangeType(valueInputField.text, fieldType);  }
        protected set { valueInputField.text = value.AsString(); }
    }

    void Start()
    {
        valueInputField.onEndEdit.AddListener(OnSubmit);
    }

    void OnDestroy()
    {
        valueInputField.onEndEdit.RemoveListener(OnSubmit);
    }

    void Update()
    {
        if (!valueInputField.isFocused) {
            value = componentType.GetField(fieldName).GetValue(component);
        }
    }

    void OnSubmit(string text)
    {
        componentType.GetField(fieldName).SetValue(component, value);
    }
}

}
