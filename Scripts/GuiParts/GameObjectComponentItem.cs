using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class GameObjectComponentItem : MonoBehaviour
{
	[HideInInspector]
	public MonoBehaviour component;
	[HideInInspector]
	public System.Type type;

	public Text nameText;
	public Button button;

	public string title
	{
		get { return nameText.text;  }
		set { nameText.text = value; }
	}

	void Start()
	{
		button.onClick.AddListener(OnClick);
	}

	void Destroy()
	{
		button.onClick.RemoveListener(OnClick);
	}

	void OnClick()
	{
		Inspector.Inspect(component, type);
	}
}

}
