using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class ComponentItem : MonoBehaviour
{
	[HideInInspector]
	public MonoBehaviour component;

	public Toggle toggle;
	public Text nameText;
	public Transform fieldsView;
	public GameObject noAvailableFieldText;

	public string title
	{
		get { return nameText.text;  }
		set { nameText.text = value; }
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
		if (component) {
			toggle.isOn = component.enabled;
		}
	}

	void OnValueChanged(bool isOn)
	{
		if (component) {
			component.enabled = isOn;
		}
	}
}

}
