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
                posX.text.AsFloat(),
                posY.text.AsFloat(),
                posZ.text.AsFloat());
        }
        protected set
        {
            posX.text = value.x.AsString();
            posY.text = value.y.AsString();
            posZ.text = value.z.AsString();
        }
    }

    public Quaternion rotation
    {
        get
        {
            return Quaternion.Euler(
                rotX.text.AsFloat(),
                rotY.text.AsFloat(),
                rotZ.text.AsFloat());
        }
        protected set
        {
            var euler = value.eulerAngles;
            rotX.text = euler.x.AsString();
            rotY.text = euler.y.AsString();
            rotZ.text = euler.z.AsString();
        }
    }

    public Vector3 scale
    {
        get
        {
            return new Vector3(
                scaleX.text.AsFloat(),
                scaleY.text.AsFloat(),
                scaleZ.text.AsFloat());
        }
        protected set
        {
            scaleX.text = value.x.AsString();
            scaleY.text = value.y.AsString();
            scaleZ.text = value.z.AsString();
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

