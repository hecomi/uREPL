using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

namespace UnityShell
{

public class Gui : MonoBehaviour
{
	static public Gui instance;
	private History history_ = new History();

	public InputField input;
	public Text output;

	public CompletionView completionView;
	public float completionTimer = 0.5f;
	private bool isComplementing_ = false;
	private bool isCompletionStopped_ = false;
	private float elapsedTimeFromLastInput_ = 0f;

	private string partial_ = "";
	private string completionPrefix_ = "";

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
		if (input.isFocused) {
			CheckCommands();
			CheckEmacsLikeCommands();
		}

		UpdateCompletion();
	}

	void CheckCommands()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			if (isComplementing_) {
				completionView.Next();
				input.MoveTextEnd(false);
			} else {
				if (history_.IsFirst()) history_.SetInputting(input.text);
				input.text = history_.Prev();
				input.MoveTextEnd(false);
			}
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			if (isComplementing_) {
				completionView.Prev();
				input.MoveTextEnd(false);
			} else {
				input.text = history_.Next();
				input.MoveTextEnd(false);
			}
		}
		if (Input.GetKeyDown(KeyCode.Tab)) {
			isCompletionStopped_ = false;
			if (isComplementing_) {
				DoCompletion();
			} else {
				SetCompletions();
			}
		}
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
			if (isComplementing_) {
				DoCompletion();
			} else {
				OnSubmit(input.text);
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape)) {
			StopCompletion();
		}
	}

	void CheckEmacsLikeCommands()
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
			if (Input.GetKeyDown(KeyCode.P)) {
				if (isComplementing_) {
					completionView.Next();
				} else {
					if (history_.IsFirst()) history_.SetInputting(input.text);
					input.text = history_.Prev();
					input.MoveTextEnd(false);
				}
			}
			if (Input.GetKeyDown(KeyCode.N)) {
				if (isComplementing_) {
					completionView.Prev();
				} else {
					input.text = history_.Next();
					input.MoveTextEnd(false);
				}
			}
			if (Input.GetKeyDown(KeyCode.F)) {
				input.caretPosition = Mathf.Min(input.caretPosition + 1, input.text.Length);
			}
			if (Input.GetKeyDown(KeyCode.B)) {
				input.caretPosition = Mathf.Max(input.caretPosition - 1, 0);
			}
			if (Input.GetKeyDown(KeyCode.A)) {
				input.MoveTextStart(false);
			}
			if (Input.GetKeyDown(KeyCode.E)) {
				input.MoveTextEnd(false);
			}
			if (Input.GetKeyDown(KeyCode.H)) {
				if (input.caretPosition > 0) {
					input.text = input.text.Remove(input.caretPosition - 1, 1);
				}
			}
			if (Input.GetKeyDown(KeyCode.D)) {
				if (input.caretPosition < input.text.Length) {
					input.text = input.text.Remove(input.caretPosition, 1);
				}
			}
			if (Input.GetKeyDown(KeyCode.K)) {
				if (input.caretPosition < input.text.Length) {
					input.text = input.text.Remove(input.caretPosition);
				}
			}
		}
	}

	private void UpdateCompletion()
	{
		// show completion view after waiting for completionTimer.
		if (!isCompletionStopped_ && !isComplementing_) {
			elapsedTimeFromLastInput_ += Time.deltaTime;
			if (elapsedTimeFromLastInput_ >= completionTimer) {
				SetCompletions();
			}
		}

		// update completion view position.
		completionView.position = GetCompletionPosition();
	}

	private void SetCompletions()
	{
		if (string.IsNullOrEmpty(input.text)) return;

		var completions = Core.GetCompletions(input.text, out completionPrefix_);
		if (completions != null && completions.Length > 0) {
			completionView.UpdateCompletion(completions, completionPrefix_);
			isComplementing_ = true;
		}
	}

	private void DoCompletion()
	{
		input.text += completionView.selectedCode;
		completionView.Reset();

		input.Select();
		input.MoveTextEnd(false);

		StopCompletion();
	}

	private void ResetCompletion()
	{
		isComplementing_ = false;
		isCompletionStopped_ = false;
		elapsedTimeFromLastInput_ = 0;
		completionView.Reset();
	}

	private void StopCompletion()
	{
		isComplementing_ = false;
		isCompletionStopped_ = true;
		completionView.Reset();
		// completionView.Hide();
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

	private bool IsInputContinuously()
	{
		return
			Input.GetKey(KeyCode.LeftShift) ||
			Input.GetKey(KeyCode.RightShift);
	}

	private bool IsEnterPressing()
	{
		return
			Input.GetKeyDown(KeyCode.Return) ||
			Input.GetKeyDown(KeyCode.KeypadEnter);
	}

	public Vector3 GetCompletionPosition()
	{
		if (input.isFocused) {
			var generator = input.textComponent.cachedTextGenerator;
			if (input.caretPosition < generator.characters.Count) {
				var len = input.text.Length;
				var info = generator.characters[len];
				var ppu  = input.textComponent.pixelsPerUnit;
				var x = info.cursorPos.x / ppu;
				var y = info.cursorPos.y / ppu;
				var prefixWidth = 0f;
				for (int i = 0; i < completionPrefix_.Length && i < len; ++i) {
					prefixWidth += generator.characters[len - 1 - i].charWidth;
				}
				prefixWidth /= ppu;
				var inputTform = input.GetComponent<RectTransform>();
				return inputTform.localPosition + new Vector3(x - prefixWidth, y, 0);
			}
		}
		return -9999f * Vector3.one;
	}

	private void OnValueChanged(string text)
	{
		ResetCompletion();
	}

	private void OnSubmit(string text)
	{
		// do nothing if following states:
		// - the input text is empty.
		// - receive the endEdit event without the enter key (e.g. lost focus).
		// - the completion box is active.
		if (string.IsNullOrEmpty(text) || !IsEnterPressing() || isComplementing_) return;

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
				history_.Add(result.code);
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

		input.ActivateInputField();
		input.Select();
		isComplementing_ = false;
	}
}

}
