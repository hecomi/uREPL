using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class CommandView : MonoBehaviour
{
	private CommandInputField inputField_;
	public CommandInputField inputField
	{
		get { return inputField_;  }
		set { inputField_ = value; }
	}

	private Button lineTypeToggleButton_;
	private Image lineTypeIcon_;

	private Sprite singleLineIcon_;
	private Sprite multiLineIcon_;

	[SerializeField] float singleLineHeight = 30f;
	[SerializeField] float multiLineHeight = 100f;

	public void Initialize(Window window)
	{
		inputField = transform.Find("Input Field").GetComponent<CommandInputField>();
		inputField.parentWindow = window;

		lineTypeToggleButton_ = transform.Find("Line Type Toggle Button").GetComponent<Button>();
		lineTypeIcon_ = lineTypeToggleButton_.transform.Find("Line Type Icon").GetComponent<Image>();

		singleLineIcon_ = Resources.Load<Sprite>("uREPL/Images/SingleLine");
		multiLineIcon_ = Resources.Load<Sprite>("uREPL/Images/MultiLine");
	}

	void Update()
	{
		if (inputField.multiLine) {
			lineTypeIcon_.sprite = multiLineIcon_;
		} else {
			lineTypeIcon_.sprite = singleLineIcon_;
		}
	}

	public void ToggleLineType()
	{
		var layout = GetComponent<LayoutElement>();
		if (inputField.multiLine) {
			inputField.lineType = InputField.LineType.SingleLine;
			if (layout) layout.minHeight = singleLineHeight;
			inputField.text = inputField.text.Replace('\n', ' ');
		} else {
			inputField.lineType = InputField.LineType.MultiLineNewline;
			if (layout) layout.minHeight = multiLineHeight;
		}
	}
}

}