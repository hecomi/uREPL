using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

public class ReadonlyFieldItem : FieldItem
{
	public Text valueText;

	public override object value
	{
		get { return Convert.ChangeType(valueText.text, fieldType);  }
		protected set { valueText.text = value.ToString(); }
	}

	void Update()
	{
		value = componentType.GetField(fieldName).GetValue(component);
	}
}
