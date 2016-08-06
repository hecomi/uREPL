using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class GameObjectComponentItem : MonoBehaviour
{
	[HideInInspector]
	public Component component;
	[HideInInspector]
	public System.Type type;

	public Text nameText;
	public Text detailText;
	public Button button;

	public string title
	{
		get { return nameText.text;  }
		set { nameText.text = value; }
	}

	public string detail
	{
		get { return detailText.text;  }
		set { detailText.text = value; }
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
