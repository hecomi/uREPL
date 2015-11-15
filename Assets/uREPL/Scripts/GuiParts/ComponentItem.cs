using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class ComponentItem : MonoBehaviour
{
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
}

}
