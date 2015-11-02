using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityShell
{

public class History
{
	private const string key = "unityshell_command_history";
	private const string separator = "<>";
	private const int maxNum = 100;

	private List<string> codes_ = new List<string>();
	private int index_ = -1;
	private string inputting_ = "";

	public int Count
	{
		get { return codes_.Count; }
	}

	public bool IsFirst()
	{
		return index_ == -1;
	}

	public string Get()
	{
		Debug.Log(index_ + " " + Count + " " + (index_ == -1 ? inputting_ : codes_[index_]));
		return index_ == -1 ? inputting_ : codes_[index_];
	}

	public string Next()
	{
		if (index_ > -1) --index_;
		return Get();
	}

	public string Prev()
	{
		if (index_ < Count - 1) ++index_;
		return Get();
	}

	public void SetInputting(string code)
	{
		inputting_ = code;
	}

	public void Reset()
	{
		index_ = -1;
		inputting_ = "";
	}

	public void Add(string code)
	{
		if (Count > 0 && codes_[Count - 1] == code) return;
		codes_.Insert(0, code);
		while (Count > maxNum) {
			codes_.RemoveAt(Count - 1);
		}
	}

	public void Clear()
	{
		PlayerPrefs.DeleteKey(key);
		codes_.Clear();
	}

	public void Save()
	{
		if (Count > 0) {
			var str = codes_.Aggregate((a, b) => a + separator + b);
			PlayerPrefs.SetString(key, str);
		}
	}

	public void Load()
	{
		var str = PlayerPrefs.GetString(key);
		if (!string.IsNullOrEmpty(str)) {
			codes_ = str.Split(
				new string[] { separator },
				System.StringSplitOptions.RemoveEmptyEntries).ToList();
		}
	}
}

public class Gui : MonoBehaviour
{
	static public Gui instance;
	private History history_ = new History();

	public InputField input;
	public Text output;
	private string partial_ = "";

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		RegisterListeners();
		history_.Load();
	}

	void OnDestroy()
	{
		UnregisterListeners();
		history_.Save();
	}

	void Update()
	{
		if (IsFocused()) {
			CheckCommands();
			CheckEmacsLikeCommands();
		}
	}

	void CheckCommands()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			if (history_.IsFirst()) history_.SetInputting(input.text);
			input.text = history_.Prev();
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			input.text = history_.Next();
		}
	}

	void CheckEmacsLikeCommands()
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
			if (Input.GetKeyDown(KeyCode.P)) {
				if (history_.IsFirst()) history_.SetInputting(input.text);
				input.text = history_.Prev();
				input.MoveTextEnd(false);
			}
			if (Input.GetKeyDown(KeyCode.N)) {
				input.text = history_.Next();
				input.MoveTextEnd(false);
			}
			if (Input.GetKeyDown(KeyCode.F)) {
				input.caretPosition++;
			}
			if (Input.GetKeyDown(KeyCode.B)) {
				input.caretPosition--;
			}
			if (Input.GetKeyDown(KeyCode.A)) {
				input.MoveTextStart(false);
			}
			if (Input.GetKeyDown(KeyCode.E)) {
				input.MoveTextEnd(false);
			}
			if (Input.GetKeyDown(KeyCode.H)) {
				input.text = input.text.Remove(input.caretPosition - 1, 1);
				input.caretPosition--;
			}
			if (Input.GetKeyDown(KeyCode.D)) {
				input.text = input.text.Remove(input.caretPosition, 1);
			}
			if (Input.GetKeyDown(KeyCode.K)) {
				input.text = input.text.Remove(input.caretPosition);
			}
		}
	}

	private void RegisterListeners()
	{
		input.onValueChange.AddListener(OnValueChanged);
		input.onEndEdit.AddListener(OnSubmit);
	}

	private void UnregisterListeners()
	{
		input.onValueChange.RemoveListener(OnValueChanged);
		input.onEndEdit.RemoveListener(OnSubmit);
	}

	public static void ClearHistory()
	{
		instance.history_.Clear();
	}

	private void Clear()
	{
		partial_    = "";
		input.text  = "";
		output.text = "";
	}

	private bool IsInputContinuously()
	{
		return
			Input.GetKey(KeyCode.LeftShift) ||
			Input.GetKey(KeyCode.RightShift);
	}

	private bool IsFocused()
	{
		return EventSystem.current.currentSelectedGameObject == input.gameObject;
	}

	private bool IsEnterPressing()
	{
		return
			Input.GetKeyDown(KeyCode.Return) ||
			Input.GetKeyDown(KeyCode.KeypadEnter);
	}

	private void OnValueChanged(string text)
	{
		// TODO: completion
	}

	private void OnSubmit(string text)
	{
		if (string.IsNullOrEmpty(text) || !IsEnterPressing()) return;

		var isPartial = false;
		var code = text;
		if (!string.IsNullOrEmpty(partial_)) {
			code = partial_ + code;
			partial_ = "";
			isPartial = true;
		}

		if ((code.Substring(code.Length - 1) != ";") && !IsInputContinuously()) {
			code += ";";
		}

		// TODO: insert pre-defined commands evaluation here.
		var result = Core.Evaluate(code);

		switch (result.type) {
			case CompileResult.Type.Success: {
				input.text = "";
				history_.Add(code);
				history_.Reset();
				output.text += string.Format("<color=white>{0}</color>\n", isPartial ? text : result.code);
				output.text += string.Format("> <color=green>{0}</color>\n", result.value);
				break;
			}
			case CompileResult.Type.Partial: {
				input.text = "";
				partial_ += text;
				output.text += string.Format("<color=white>{0}</color>\n", text);
				output.text += string.Format(">> ", text);
				break;
			}
			case CompileResult.Type.Error: {
				output.text += string.Format("<color=white>{0}</color>\n", result.code);
				output.text += string.Format("> <color=red>{0}</color>\n", result.error);
				break;
			}
		}

		input.Select();
		input.ActivateInputField();
	}
}

}
