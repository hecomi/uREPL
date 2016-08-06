using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace uREPL
{

public class BoolFieldItem : FieldItem
{
	public Toggle toggle;

	public override object value
	{
		get { return toggle.isOn;  }
		protected set { toggle.isOn = (bool)value; }
	}

	void Start()
	{
		toggle.onValueChanged.AddListener(OnValueChanged);
	}

	void OnDestroy()
	{
		toggle.onValueChanged.RemoveListener(OnValueChanged);
	}

	void Update()
	{
		value = componentType.GetField(fieldName).GetValue(component);
	}

	void OnValueChanged(bool isOn)
	{
		componentType.GetField(fieldName).SetValue(component, isOn);
	}
}

}
