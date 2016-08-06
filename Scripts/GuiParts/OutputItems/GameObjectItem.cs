using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class GameObjectItem : MonoBehaviour
{
	[HideInInspector]
	public GameObject targetGameObject;

	public Toggle toggle;
	public Text nameText;
	public InputField posX, posY, posZ;
	public InputField rotX, rotY, rotZ;
	public InputField scaleX, scaleY, scaleZ;
	public Transform componentsFieldsView;
	public GameObject noAvailableComponentText;
	public GameObject componentItemPrefab;

	public string title
	{
		get { return nameText.text;  }
		set { nameText.text = value; }
	}

	public Vector3 position
	{
		get
		{
			return new Vector3(
				float.Parse(posX.text),
				float.Parse(posY.text),
				float.Parse(posZ.text));
		}
		protected set
		{
			posX.text = value.x.ToString();
			posY.text = value.y.ToString();
			posZ.text = value.z.ToString();
		}
	}

	public Quaternion rotation
	{
		get
		{
			return Quaternion.Euler(
				float.Parse(rotX.text),
				float.Parse(rotY.text),
				float.Parse(rotZ.text));
		}
		protected set
		{
			var euler = value.eulerAngles;
			rotX.text = euler.x.ToString();
			rotY.text = euler.y.ToString();
			rotZ.text = euler.z.ToString();
		}
	}

	public Vector3 scale
	{
		get
		{
			return new Vector3(
				float.Parse(scaleX.text),
				float.Parse(scaleY.text),
				float.Parse(scaleZ.text));
		}
		protected set
		{
			scaleX.text = value.x.ToString();
			scaleY.text = value.y.ToString();
			scaleZ.text = value.z.ToString();
		}
	}

	void Start()
	{
		posX.onEndEdit.AddListener(OnSubmitPosition);
		posY.onEndEdit.AddListener(OnSubmitPosition);
		posZ.onEndEdit.AddListener(OnSubmitPosition);
		rotX.onEndEdit.AddListener(OnSubmitRotation);
		rotY.onEndEdit.AddListener(OnSubmitRotation);
		rotZ.onEndEdit.AddListener(OnSubmitRotation);
		scaleX.onEndEdit.AddListener(OnSubmitScale);
		scaleY.onEndEdit.AddListener(OnSubmitScale);
		scaleZ.onEndEdit.AddListener(OnSubmitScale);

		toggle.onValueChanged.AddListener(OnValueChanged);

		if (targetGameObject) {
			var components = targetGameObject.GetComponents<Component>();
			foreach (var component in components) {
				noAvailableComponentText.SetActive(false);
				var obj = Instantiate(componentItemPrefab);
				obj.transform.SetParent(componentsFieldsView);
				var item = obj.GetComponent<GameObjectComponentItem>();
				item.component = component;
				item.type = component.GetType();
				item.title = item.type.Name;
				item.detail = "(" + item.type.FullName + ")";
			}
		}
	}

	void OnDestroy()
	{
		posX.onEndEdit.RemoveListener(OnSubmitPosition);
		posY.onEndEdit.RemoveListener(OnSubmitPosition);
		posZ.onEndEdit.RemoveListener(OnSubmitPosition);
		rotX.onEndEdit.RemoveListener(OnSubmitRotation);
		rotY.onEndEdit.RemoveListener(OnSubmitRotation);
		rotZ.onEndEdit.RemoveListener(OnSubmitRotation);
		scaleX.onEndEdit.RemoveListener(OnSubmitScale);
		scaleY.onEndEdit.RemoveListener(OnSubmitScale);
		scaleZ.onEndEdit.RemoveListener(OnSubmitScale);

		toggle.onValueChanged.RemoveListener(OnValueChanged);
	}

	void Update()
	{
		if (targetGameObject) {
			toggle.isOn = targetGameObject.activeSelf;

			if (!posX.isFocused &&
				!posY.isFocused &&
				!posZ.isFocused) {
				position = targetGameObject.transform.position;
			}
			if (!rotX.isFocused &&
				!rotY.isFocused &&
				!rotZ.isFocused) {
				rotation = targetGameObject.transform.rotation;
			}
			if (!scaleX.isFocused &&
				!scaleY.isFocused &&
				!scaleZ.isFocused) {
				scale = targetGameObject.transform.localScale;
			}
		}
	}

	void OnValueChanged(bool isOn)
	{
		if (targetGameObject) {
			targetGameObject.SetActive(isOn);
		}
	}

	void OnSubmitPosition(string text)
	{
		targetGameObject.transform.position = position;
	}

	void OnSubmitRotation(string text)
	{
		targetGameObject.transform.rotation = rotation;
	}

	void OnSubmitScale(string text)
	{
		targetGameObject.transform.localScale = scale;
	}
}

}

