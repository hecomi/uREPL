using UnityEngine;
using UnityEngine.UI;

public abstract class FieldItem : MonoBehaviour
{
	protected Text labelText;

	public string label
	{
		get { return labelText.text;  }
		set { labelText.text = value; }
	}

	public abstract object value
	{
		get;
		protected set;
	}

	[HideInInspector]
	public Component component;
	[HideInInspector]
	public System.Type componentType;
	[HideInInspector]
	public string fieldName;
	[HideInInspector]
	public System.Type fieldType;

	void Awake()
	{
		labelText = transform.FindChild("Label").GetComponent<Text>();
	}
}
