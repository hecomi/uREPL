using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace uREPL
{

public class Gui : MonoBehaviour
{
	static public Gui instance;
	private const int resultMaxNum = 100;

	public KeyCode openKey = KeyCode.F1;
	private bool isWindowOpened_ = false;

	public InputField inputField;
	public Transform outputContent;
	public AnnotationView annotation;
	public GameObject resultItemPrefab;
	public GameObject logItemPrefab;

	public CompletionView completionView;
	public float completionTimer = 0.5f;
	private bool isComplementing_ = false;
	private bool isCompletionFinished_ = false;
	private bool isCompletionStopped_ = false;
	private float elapsedTimeFromLastInput_ = 0f;

	public float annotationTimer = 1f;
	private float elapsedTimeFromLastSelect_ = 0f;

	private string partial_ = "";
	private string currentComletionPrefix_ = "";

	private History history_ = new History();

	private Thread completionThread_;
	private CompletionInfo[] completions_;

	private enum KeyOption {
		None  = 0,
		Ctrl  = 1000,
		Shift = 1000000,
		Alt   = 1000000000
	};
	private Dictionary<int, int> keyPressingCounter_ = new Dictionary<int, int>();
	public int holdInputStartDelay = 30;
	public int holdInputFrameInterval = 5;

	public Queue<Log.Data> logData_ = new Queue<Log.Data>();

	void Awake()
	{
		instance = this;
		Core.Initialize();
		isWindowOpened_ = GetComponent<Canvas>().enabled;
	}

	void Start()
	{
		RegisterListeners();
		history_.Load();
	}

	void OnDestroy()
	{
		instance = null;
		StopCompletionThread();
		UnregisterListeners();
		history_.Save();
	}

	void Update()
	{
		if (Input.GetKeyDown(openKey)) {
			OpenWindow();
		}

		if (isWindowOpened_) {
			if (inputField.isFocused) {
				CheckCommands();
				CheckEmacsLikeCommands();
			}
			UpdateCompletion();
		}

		UpdateLogs();
		UpdateAnnotation();
	}

	public void OpenWindow()
	{
		GetComponent<Canvas>().enabled = true;
		RunOnNextFrame(() => inputField.Select());
		isWindowOpened_ = true;
	}

	public void CloseWindow()
	{
		GetComponent<Canvas>().enabled = false;
		isWindowOpened_ = false;
	}

	public void ClearOutputView()
	{
		for (int i = 0; i < outputContent.childCount; ++i) {
			Destroy(outputContent.GetChild(i).gameObject);
		}
	}

	private void Prev()
	{
		if (isComplementing_) {
			completionView.Next();
			ResetAnnotation();
		} else {
			if (history_.IsFirst()) history_.SetInputtingCommand(inputField.text);
			inputField.text = history_.Prev();
			isCompletionStopped_ = true;
		}
		inputField.MoveTextEnd(false);
	}

	private void Next()
	{
		if (isComplementing_) {
			completionView.Prev();
			ResetAnnotation();
		} else {
			inputField.text = history_.Next();
			isCompletionStopped_ = true;
		}
		inputField.MoveTextEnd(false);
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
			(cnt >= holdInputStartDelay && cnt % holdInputFrameInterval == 0);
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
			if (isComplementing_) {
				DoCompletion();
			} else {
				ResetCompletion();
				StartCompletion();
			}
		}
		if (IsEnterPressing()) {
			if (isComplementing_ && !IsInputContinuously()) {
				DoCompletion();
			} else {
				OnSubmit(inputField.text);
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
			inputField.caretPosition = Mathf.Min(inputField.caretPosition + 1, inputField.text.Length);
		}
		if (CheckKey(KeyCode.B, KeyOption.Ctrl)) {
			inputField.caretPosition = Mathf.Max(inputField.caretPosition - 1, 0);
		}
		if (CheckKey(KeyCode.A, KeyOption.Ctrl)) {
			inputField.MoveTextStart(false);
		}
		if (CheckKey(KeyCode.E, KeyOption.Ctrl)) {
			inputField.MoveTextEnd(false);
		}
		if (CheckKey(KeyCode.H, KeyOption.Ctrl)) {
			if (inputField.caretPosition > 0) {
				var isCaretPositionLast = inputField.caretPosition == inputField.text.Length;
				inputField.text = inputField.text.Remove(inputField.caretPosition - 1, 1);
				if (!isCaretPositionLast) {
					--inputField.caretPosition;
				}
			}
		}
		if (CheckKey(KeyCode.D, KeyOption.Ctrl)) {
			if (inputField.caretPosition < inputField.text.Length) {
				inputField.text = inputField.text.Remove(inputField.caretPosition, 1);
			}
		}
		if (CheckKey(KeyCode.K, KeyOption.Ctrl)) {
			if (inputField.caretPosition < inputField.text.Length) {
				inputField.text = inputField.text.Remove(inputField.caretPosition);
			}
		}
		if (CheckKey(KeyCode.L, KeyOption.Ctrl)) {
			ClearOutputView();
		}
	}

	private void UpdateCompletion()
	{
		elapsedTimeFromLastInput_ += Time.deltaTime;

		// stop completion thread if it is running to avoid hang.
		if (IsCompletionThreadAlive() &&
			elapsedTimeFromLastInput_ > completionTimer + 0.5f /* margin */) {
			StopCompletion();
		}

		// show completion view after waiting for completionTimer.
		if (!isCompletionStopped_ && !isCompletionFinished_ && !IsCompletionThreadAlive()) {
			if (elapsedTimeFromLastInput_ >= completionTimer) {
				StartCompletion();
			}
		}

		// update completion view position.
		completionView.position = GetCompletionPosition();

		// update completion view if new completions set.
		if (completions_ != null && completions_.Length > 0) {
			completionView.SetCompletions(completions_);
			completions_ = null;
			ResetAnnotation();
		}
	}

	private void StartCompletion()
	{
		if (string.IsNullOrEmpty(inputField.text)) return;

		// avoid undesired hang caused by Mono.CSharp.GetCompletions,
		// run it on another thread and stop if hang occurs in UpdateCompletion().
		StopCompletionThread();
		StartCompletionThread();
	}

	private void StartCompletionThread()
	{
		completionThread_ = new Thread(() => {
			var code = partial_ + inputField.text;
			completions_ = Core.GetCompletions(code);
			if (completions_ != null && completions_.Length > 0) {
				currentComletionPrefix_ = completions_[0].prefix; // TODO: this is not smart...
				isComplementing_ = true;
			}
			isCompletionFinished_ = true;
		});
		completionThread_.Start();
	}

	private void StopCompletionThread()
	{
		if (completionThread_ != null) {
			completionThread_.Abort();
		}
	}

	private void DoCompletion()
	{
		inputField.text += completionView.selectedCode;
		completionView.Reset();

		inputField.Select();
		inputField.MoveTextEnd(false);

		StopCompletion();
	}

	private void ResetCompletion()
	{
		isComplementing_ = false;
		isCompletionFinished_ = false;
		elapsedTimeFromLastInput_ = 0;
		completionView.Reset();
		StopCompletionThread();
	}

	private void StopCompletion()
	{
		isComplementing_ = false;
		isCompletionStopped_ = true;
		completionView.Reset();
		StopCompletionThread();
	}

	private bool IsCompletionThreadAlive()
	{
		return completionThread_ != null && completionThread_.IsAlive;
	}

	private void RegisterListeners()
	{
		inputField.onValueChange.AddListener(OnValueChanged);
		inputField.onEndEdit.AddListener(OnSubmit);
	}

	private void UnregisterListeners()
	{
		inputField.onValueChange.RemoveListener(OnValueChanged);
		inputField.onEndEdit.RemoveListener(OnSubmit);
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
		if (inputField.isFocused && inputField.caretPosition == inputField.text.Length) {
			var generator = inputField.textComponent.cachedTextGenerator;
			if (inputField.caretPosition < generator.characters.Count) {
				var len = inputField.text.Length;
				var info = generator.characters[len];
				var ppu  = inputField.textComponent.pixelsPerUnit;
				var x = info.cursorPos.x / ppu;
				var y = info.cursorPos.y / ppu;
				var prefixWidth = 0f;
				for (int i = 0; i < currentComletionPrefix_.Length && i < len; ++i) {
					prefixWidth += generator.characters[len - 1 - i].charWidth;
				}
				prefixWidth /= ppu;
				var inputTform = inputField.GetComponent<RectTransform>();
				return inputTform.localPosition + new Vector3(x - prefixWidth, y, 0);
			}
		}
		return -9999f * Vector3.one;
	}

	private void OnValueChanged(string text)
	{
		text = text.Replace("\n", "");
		text = text.Replace("\r", "");
		inputField.text = text;
		if (!IsEnterPressing()) {
			isCompletionStopped_ = false;
			RunOnEndOfFrame(() => { ResetCompletion(); });
		}
	}

	private void OnSubmit(string text)
	{
		text = text.Trim();

		// do nothing if following states:
		// - the input text is empty.
		// - receive the endEdit event without the enter key (e.g. lost focus).
		if (string.IsNullOrEmpty(text) || !IsEnterPressing()) return;

		// stop completion to avoid hang.
		StopCompletionThread();

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

		var result = Core.Evaluate(code);
		ResultItem view = null;
		if (isPartial) {
			view = outputContent.GetChild(outputContent.childCount - 1).GetComponent<ResultItem>();
		} else {
			view = Instantiate(resultItemPrefab).GetComponent<ResultItem>();
			view.transform.SetParent(outputContent);
		}
		RemoveExceededItem();

		switch (result.type) {
			case CompileResult.Type.Success: {
				inputField.text = "";
				history_.Add(result.code);
				history_.Reset();
				view.type   = CompileResult.Type.Success;
				view.input  = result.code;
				view.output = result.value.ToString();
				break;
			}
			case CompileResult.Type.Partial: {
				inputField.text = "";
				partial_ += text;
				view.type   = CompileResult.Type.Partial;
				view.input  = result.code;
				view.output = "...";
				break;
			}
			case CompileResult.Type.Error: {
				view.type   = CompileResult.Type.Error;
				view.input  = result.code;
				view.output = result.error;
				break;
			}
		}

		isComplementing_ = false;
	}

	private void RemoveExceededItem()
	{
		if (outputContent.childCount > resultMaxNum) {
			Destroy(outputContent.GetChild(0).gameObject);
		}
	}

	public void OutputLog(Log.Data data)
	{
		logData_.Enqueue(data);
	}

	private void UpdateLogs()
	{
		while (logData_.Count > 0) {
			var data = logData_.Dequeue();
			var item = Instantiate(logItemPrefab).GetComponent<LogItem>();
			item.transform.SetParent(outputContent);
			item.level = data.level;
			item.log   = data.log;
			item.meta  = data.meta;
		}
		RemoveExceededItem();
	}

	private void ResetAnnotation()
	{
		elapsedTimeFromLastSelect_ = 0f;
	}

	private void UpdateAnnotation()
	{
		elapsedTimeFromLastSelect_ += Time.deltaTime;

		var item = completionView.selectedItem;
		var hasDescription = (item != null) && item.hasDescription;
		if (hasDescription) annotation.text = item.description;

		var isAnnotationVisible = elapsedTimeFromLastSelect_ >= annotationTimer;
		annotation.gameObject.SetActive(isAnnotationVisible && isComplementing_ && hasDescription);

		annotation.transform.position =
			completionView.selectedPosition + Vector3.right * (completionView.width + 4f);
	}

	private void RunOnEndOfFrame(System.Action func)
	{
		StartCoroutine(_RunOnEndOfFrame(func));
	}

	private IEnumerator _RunOnEndOfFrame(System.Action func)
	{
		yield return new WaitForEndOfFrame();
		func();
	}

	private void RunOnNextFrame(System.Action func)
	{
		StartCoroutine(_RunOnNextFrame(func));
	}

	private IEnumerator _RunOnNextFrame(System.Action func)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		func();
	}

	[Command(command = "quit", description = "Close console.")]
	static public void QuitCommand()
	{
		instance.CloseWindow();
	}

	[Command(command = "open window", description = "Open console.")]
	static public void OpenCommand()
	{
		instance.OpenWindow();
	}

	[Command(command = "clear outputs", description = "Clear output view.")]
	static public void ClearOutputCommand()
	{
		instance.RunOnNextFrame(() => {
			instance.ClearOutputView();
		});
	}

	[Command(command = "clear histories", description = "Clear all input histories.")]
	static public void ClearHistoryCommand()
	{
		instance.RunOnNextFrame(() => {
			instance.history_.Clear();
		});
	}

	[Command(command = "show histories", description = "show command histoies.")]
	static public void ShowHistory()
	{
		string histories = "";
		int num = instance.history_.Count;
		foreach (var command in instance.history_.list.ToArray().Reverse()) {
			histories += string.Format("{0}: {1}\n", num, command);
			--num;
		}
		Log.Output(histories);
	}
}

}
