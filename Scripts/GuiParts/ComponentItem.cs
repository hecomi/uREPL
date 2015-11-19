using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class ComponentItem : MonoBehaviour
{
	[HideInInspector]
	public Component component;

	public Toggle toggle;
	public GameObject checkbox;

	public Text nameText;
	public Transform fieldsView;
	public GameObject noAvailableFieldText;

	public string title
	{
		get { return nameText.text;  }
		set { nameText.text = value; }
	}

	public System.Type type
	{
		get { return component.GetType(); }
	}

	public bool hasEnabled
	{
		get { return component && type.GetProperty("enabled") != null; }
	}

	public void Add(GameObject field)
	{
		noAvailableFieldText.SetActive(false);
		field.transform.SetParent(fieldsView);
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
		if (hasEnabled) {
			toggle.isOn = (bool)type.GetProperty("enabled").GetValue(component, null);
			checkbox.SetActive(true);
		} else {
			checkbox.SetActive(false);
		}
	}

	void OnValueChanged(bool isOn)
	{
		if (hasEnabled) {
			type.GetProperty("enabled").SetValue(component, isOn, null);
		}
	}
}

}
