using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace uREPL
{

public class EnumFieldItem : FieldItem
{
	public Dropdown dropdown;

	public override object value
	{
		get
		{
			return Convert.ChangeType(
				Enum.Parse(
					typeof(KeyCode),
					dropdown.captionText.text),
				typeof(KeyCode));
		}
		protected set
		{
			dropdown.value = dropdown.options.FindIndex(item => {
				return item.text == value.ToString();
			});
		}
	}

	private object preValue_ = null;

	void Start()
	{
		dropdown.onValueChanged.AddListener(OnValueChanged);
		Init();
	}

	void OnDestroy()
	{
		dropdown.onValueChanged.RemoveListener(OnValueChanged);
	}

	void Init()
	{
		fieldType = typeof(KeyCode);

		dropdown.options.Clear();
		foreach (var value in Enum.GetValues(fieldType)) {
			var item = new Dropdown.OptionData();
			item.text = value.ToString();
			dropdown.options.Add(item);
		}
		dropdown.captionText.text = dropdown.options[dropdown.value].text;
	}

	void Update()
	{
		var newValue = componentType.GetField(fieldName).GetValue(component);
		if (preValue_ != newValue) {
			value = preValue_ = newValue;
		}
	}

	void OnValueChanged(int index)
	{
		componentType.GetField(fieldName).SetValue(component, value);
	}
}

}
