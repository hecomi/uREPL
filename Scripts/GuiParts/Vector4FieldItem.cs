using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace uREPL
{

public class Vector4FieldItem : FieldItem
{
	public InputField xInputField;
	public InputField yInputField;
	public InputField zInputField;
	public InputField wInputField;

	public override object value
	{
		get
		{
			return Activator.CreateInstance(
				fieldType,
				float.Parse(xInputField.text),
				float.Parse(yInputField.text),
				float.Parse(zInputField.text),
				float.Parse(wInputField.text));
		}
		protected set
		{
			xInputField.text = fieldType.GetField("x").GetValue(value).ToString();
			yInputField.text = fieldType.GetField("y").GetValue(value).ToString();
			zInputField.text = fieldType.GetField("z").GetValue(value).ToString();
			wInputField.text = fieldType.GetField("w").GetValue(value).ToString();
		}
	}

	void Start()
	{
		xInputField.onEndEdit.AddListener(OnSubmit);
		yInputField.onEndEdit.AddListener(OnSubmit);
		zInputField.onEndEdit.AddListener(OnSubmit);
		wInputField.onEndEdit.AddListener(OnSubmit);
	}

	void OnDestroy()
	{
		xInputField.onEndEdit.RemoveListener(OnSubmit);
		yInputField.onEndEdit.RemoveListener(OnSubmit);
		zInputField.onEndEdit.RemoveListener(OnSubmit);
		wInputField.onEndEdit.RemoveListener(OnSubmit);
	}

	void Update()
	{
		if (!xInputField.isFocused &&
			!yInputField.isFocused &&
			!zInputField.isFocused &&
			!wInputField.isFocused) {
			value = componentType.GetField(fieldName).GetValue(component);
		}
	}

	void OnSubmit(string text)
	{
		componentType.GetField(fieldName).SetValue(component, value);
	}
}

}
