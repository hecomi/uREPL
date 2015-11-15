using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

public class FieldItem : MonoBehaviour
{
	public Text labelText;
	public InputField valueInputField;

	public string label
	{
		get { return labelText.text;  }
		set { labelText.text = value; }
	}

	public string value
	{
		get { return valueInputField.text;  }
		set { valueInputField.text = value; }
	}

	[HideInInspector]
	public Component component;
	[HideInInspector]
	public System.Type componentType;
	[HideInInspector]
	public string fieldName;
	[HideInInspector]
	public System.Type fieldType;

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
			value = componentType.GetField(fieldName).GetValue(component).ToString();
		}
	}

	void OnSubmit(string text)
	{
		componentType.GetField(fieldName).SetValue(component, Convert.ChangeType(text, fieldType));
	}
}
