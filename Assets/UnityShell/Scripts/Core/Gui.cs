using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

	private Thread completionThread_;
	private string[] completions_;

	private enum KeyOption {
		None  = 0,
		Ctrl  = 1000,
		Shift = 1000000,
		Alt   = 1000000000
	};
	private Dictionary<int, int> keyPressingCounter_ = new Dictionary<int, int>();
	public int continuousInputStartDelay = 30;
	public int continuousInputFrameInterval = 5;

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
		AbortCompletion();
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

	private void Prev()
	{
		if (isComplementing_) {
			completionView.Next();
		} else {
			if (history_.IsFirst()) history_.SetInputting(input.text);
			input.text = history_.Prev();
			isCompletionStopped_ = true;
		}
		input.MoveTextEnd(false);
	}

	private void Next()
	{
		if (isComplementing_) {
			completionView.Prev();
		} else {
			input.text = history_.Next();
			isCompletionStopped_ = true;
		}
		input.MoveTextEnd(false);
	}

	private bool CheckKey(KeyCode keyCode, KeyOption option = KeyOption.None)
	{
		var key = (int)keyCode + (int)option;
		if (!keyPressingCounter_.ContainsKey(key)) {
			keyPressingCounter_.Add(key, 0);
		}

		bool isOptionAcceptable = false;
		switch (option) {
			case KeyOption.None:
				isOptionAcceptable = true;
				break;
			case KeyOption.Ctrl:
				isOptionAcceptable =
					Input.GetKey(KeyCode.LeftControl) ||
					Input.GetKey(KeyCode.RightControl);
				break;
			case KeyOption.Shift:
				isOptionAcceptable =
					Input.GetKey(KeyCode.LeftShift) ||
					Input.GetKey(KeyCode.RightShift);
				break;
			case KeyOption.Alt:
				isOptionAcceptable =
					Input.GetKey(KeyCode.LeftAlt) ||
					Input.GetKey(KeyCode.RightAlt);
				break;
		}

		var cnt = keyPressingCounter_[key];
		if (Input.GetKey(keyCode) && isOptionAcceptable) {
			++cnt;
		} else {
			cnt = 0;
		}
		keyPressingCounter_[key] = cnt;

		return
			cnt == 1 ||
			(cnt >= continuousInputStartDelay && cnt % continuousInputFrameInterval == 0);
	}

	private void CheckCommands()
	{
		if (CheckKey(KeyCode.UpArrow)) {
			Prev();
		}
		if (CheckKey(KeyCode.DownArrow)) {
			Next();
		}
		if (CheckKey(KeyCode.Tab)) {
			isCompletionStopped_ = false;
			if (isComplementing_) {
				DoCompletion();
			} else {
				SetCompletions();
			}
		}
		if (CheckKey(KeyCode.Return) || CheckKey(KeyCode.KeypadEnter)) {
			if (isComplementing_) {
				DoCompletion();
			} else {
				OnSubmit(input.text);
			}
		}
		if (CheckKey(KeyCode.Escape)) {
			StopCompletion();
		}
	}

	void CheckEmacsLikeCommands()
	{
		if (CheckKey(KeyCode.P, KeyOption.Ctrl)) {
			Prev();
		}
		if (CheckKey(KeyCode.N, KeyOption.Ctrl)) {
			Next();
		}
		if (CheckKey(KeyCode.F, KeyOption.Ctrl)) {
			input.caretPosition = Mathf.Min(input.caretPosition + 1, input.text.Length);
		}
		if (CheckKey(KeyCode.B, KeyOption.Ctrl)) {
			input.caretPosition = Mathf.Max(input.caretPosition - 1, 0);
		}
		if (CheckKey(KeyCode.A, KeyOption.Ctrl)) {
			input.MoveTextStart(false);
		}
		if (CheckKey(KeyCode.E, KeyOption.Ctrl)) {
			input.MoveTextEnd(false);
		}
		if (CheckKey(KeyCode.H, KeyOption.Ctrl)) {
			if (input.caretPosition > 0) {
				input.text = input.text.Remove(input.caretPosition - 1, 1);
				--input.caretPosition;
			}
		}
		if (CheckKey(KeyCode.D, KeyOption.Ctrl)) {
			if (input.caretPosition < input.text.Length) {
				input.text = input.text.Remove(input.caretPosition, 1);
			}
		}
		if (CheckKey(KeyCode.K, KeyOption.Ctrl)) {
			if (input.caretPosition < input.text.Length) {
				input.text = input.text.Remove(input.caretPosition);
			}
		}
	}

	private void UpdateCompletion()
	{
		elapsedTimeFromLastInput_ += Time.deltaTime;

		// stop completion thread if it is running to avoid hang.
		if (elapsedTimeFromLastInput_ > completionTimer + 0.5f /* margin */) {
			AbortCompletion();
		}

		// show completion view after waiting for completionTimer.
		if (!isCompletionStopped_ && !isComplementing_ && !IsCompletionThreadAlive()) {
			if (elapsedTimeFromLastInput_ >= completionTimer) {
				SetCompletions();
			}
		}

		// update completion view position.
		completionView.position = GetCompletionPosition();

		// update completion view if new completions set.
		if (completions_ != null && completions_.Length > 0) {
			completionView.UpdateCompletion(completions_, completionPrefix_);
			completions_ = null;
		}
	}

	private void SetCompletions()
	{
		if (string.IsNullOrEmpty(input.text)) return;

		// avoid undesired hang caused by Mono.CSharp.GetCompletions,
		// run it on anothre thread and stop if hang in UpdateCompletion().
		if (completionThread_ != null) {
			completionThread_.Abort();
		}
		completionThread_ = new Thread(() => {
			completions_ = Core.GetCompletions(input.text, out completionPrefix_);
			if (completions_ != null && completions_.Length > 0) {
				isComplementing_ = true;
			}
		});
		completionThread_.Start();
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

	private bool IsCompletionThreadAlive()
	{
		return completionThread_ != null && completionThread_.IsAlive;
	}

	private void AbortCompletion()
	{
		if (IsCompletionThreadAlive()) {
			completionThread_.Abort();
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
		if (input.isFocused && input.caretPosition == input.text.Length) {
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
		text = text.Replace("\n", "");
		text = text.Replace("\r", "");
		input.text = text;
		ResetCompletion();
	}

	private void OnSubmit(string text)
	{
		text = text.Trim();

		// do nothing if following states:
		// - the input text is empty.
		// - receive the endEdit event without the enter key (e.g. lost focus).
		// - the completion box is active.
		if (string.IsNullOrEmpty(text) || !IsEnterPressing() || isComplementing_) return;

		// stop completion to avoid hang.
		AbortCompletion();

		// use the partial code previously input if it exists.
		var isPartial = false;
		var code = text;
		if (!string.IsNullOrEmpty(partial_)) {
			code = partial_ + code;
			partial_ = "";
			isPartial = true;
		}

		// auto-complete semicolon.
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

		isComplementing_ = false;
	}
}

}
