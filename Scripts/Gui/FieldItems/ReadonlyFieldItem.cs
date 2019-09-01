using UnityEngine.UI;
using System;

namespace uREPL
{

public class ReadonlyFieldItem : FieldItem
{
    public Text valueText;

    public override object value
    {
        get { return Convert.ChangeType(valueText.text, fieldType);  }
        protected set { valueText.text = (value == null) ? "null" : value.ToString(); }
    }

    void Update()
    {
        value = componentType.GetField(fieldName).GetValue(component);
    }
}

}