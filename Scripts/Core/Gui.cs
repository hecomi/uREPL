using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace uREPL
{

public class Gui : MonoBehaviour
{
	#region [core]
	static public Gui selected;

	private Completion completion_ = new Completion();
	public Queue<Log.Data> logData_ = new Queue<Log.Data>();
	private History history_ = new History();

	private string currentComletionPrefix_ = "";

	enum CompletionState {
		Idle,
		Stop,
		WaitingForCompletion,
		Complementing,
	}
	private CompletionState completionState_ = CompletionState.Idle;
	#endregion

	#region [key operations]
	[HeaderAttribute("Keys")]
	public KeyCode openKey = KeyCode.F1;
	public KeyCode closeKey = KeyCode.F1;
	private bool isWindowOpened_ = false;

	private KeyEvent keyEvent_ = new KeyEvent();
	#endregion

	#region [content]
	private CommandInputField inputField_;
	private OutputView output_;
	private AnnotationView annotation_;
	private CompletionView completionView_;
	#endregion

	#region [parameters]
	[HeaderAttribute("Parameters")]
	public float completionTimer = 0.5f;
	public float annotationTimer = 1f;
	private float elapsedTimeFromLastInput_ = 0f;
	private float elapsedTimeFromLastSelect_ = 0f;
	#endregion

	void Awake()
	{
		InitObjects();
		InitCommands();
		InitEmacsLikeCommands();

		completion_.AddCompletionFinishedListener(OnCompletionFinished);

		Evaluator.Initialize();

		isWindowOpened_ = GetComponent<Canvas>().enabled;
		if (isWindowOpened_) {
			selected = this;
		}
	}

	void InitObjects()
	{
		// Instances
		var container   = transform.Find("Container");
		inputField_     = container.Find("Input Field").GetComponent<CommandInputField>();
		output_         = container.Find("Output View").GetComponent<OutputView>();
		annotation_     = transform.Find("Annotation View").GetComponent<AnnotationView>();
		completionView_ = transform.Find("Completion View").GetComponent<CompletionView>();

		// Settings
		inputField_.parentGui = this;
	}

	private void InitCommands()
	{
		keyEvent_.Add(KeyCode.UpArrow, Prev);
		keyEvent_.Add(KeyCode.DownArrow, Next);
		keyEvent_.Add(KeyCode.LeftArrow, StopCompletion);
		keyEvent_.Add(KeyCode.RightArrow, StopCompletion);
		keyEvent_.Add(KeyCode.Escape, StopCompletion);
		keyEvent_.Add(KeyCode.Tab, () => {
			if (completionView_.hasItem) {
				DoCompletion();
			} else {
				StartCompletion();
			}
		});
	}

	void InitEmacsLikeCommands()
	{
		keyEvent_.Add(KeyCode.P, KeyEvent.Option.Ctrl, Prev);
		keyEvent_.Add(KeyCode.N, KeyEvent.Option.Ctrl, Next);
		keyEvent_.Add(KeyCode.F, KeyEvent.Option.Ctrl, () => {
			inputField_.MoveCaretPosition(1);
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.B, KeyEvent.Option.Ctrl, () => {
			inputField_.MoveCaretPosition(-1);
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.A, KeyEvent.Option.Ctrl, () => {
			inputField_.MoveTextStart(false);
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.E, KeyEvent.Option.Ctrl, () => {
			inputField_.MoveTextEnd(false);
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.H, KeyEvent.Option.Ctrl, () => {
			inputField_.BackspaceOneCharacterFromCaretPosition();
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.D, KeyEvent.Option.Ctrl, () => {
			inputField_.DeleteOneCharacterFromCaretPosition();
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.K, KeyEvent.Option.Ctrl, () => {
			inputField_.DeleteAllCharactersAfterCaretPosition();
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.L, KeyEvent.Option.Ctrl, () => {
			output_.Clear();
		});
	}

	void Start()
	{
		RegisterListeners();
		history_.Load();
	}

	void OnDestroy()
	{
		completion_.Stop();
		UnregisterListeners();
		history_.Save();
		completion_.RemoveCompletionFinishedListener(OnCompletionFinished);
	}

	void Update()
	{
		UpdateKeyEvents();
		UpdateCompletion();
		UpdateLogs();
		UpdateAnnotation();
	}

	private void ToggleWindowByKeys()
	{
		if (openKey == closeKey) {
			if (Input.GetKeyDown(openKey)) {
				if (!isWindowOpened_) OpenWindow();
				else CloseWindow();
			}
		} else {
			if (Input.GetKeyDown(openKey)) OpenWindow();
			if (Input.GetKeyDown(closeKey)) CloseWindow();
		}
	}

	private void UpdateKeyEvents()
	{
		ToggleWindowByKeys();

		if (isWindowOpened_) {
			if (inputField_.isFocused) {
				keyEvent_.Check();
			} else {
				keyEvent_.Clear();
			}

			if (IsEnterPressing()) {
				if (completionView_.hasItem) {
					DoCompletion();
				} else {
					OnSubmit(inputField_.text);
				}
			}
		}
	}

	public void OpenWindow()
	{
		selected = this;
		SetActive(true);
		inputField_.Focus();
	}

	public void CloseWindow()
	{
		if (selected == this) {
			selected = null;
		}
		SetActive(false);
	}

	private void SetActive(bool active)
	{
		GetComponent<Canvas>().enabled = active;
		inputField_.gameObject.SetActive(active);
		output_.gameObject.SetActive(active);
		completionView_.gameObject.SetActive(active);
		isWindowOpened_ = active;
	}

	private void Prev()
	{
		if (completionView_.hasItem) {
			completionView_.Next();
			ResetAnnotation();
		} else {
			if (history_.IsFirst()) history_.SetInputtingCommand(inputField_.text);
			inputField_.text = history_.Prev();
			inputField_.MoveTextEnd(false);
			completionState_ = CompletionState.Stop;
		}
	}

	private void Next()
	{
		if (completionView_.hasItem) {
			completionView_.Prev();
			ResetAnnotation();
		} else {
			inputField_.text = history_.Next();
			inputField_.MoveTextEnd(false);
			completionState_ = CompletionState.Stop;
		}
	}

	private void UpdateCompletion()
	{
		if (!isWindowOpened_) return;

		switch (completionState_) {
			case CompletionState.Idle: {
				break;
			}
			case CompletionState.Stop: {
				completionState_ = CompletionState.Idle;
				break;
			}
			case CompletionState.WaitingForCompletion: {
				elapsedTimeFromLastInput_ += Time.deltaTime;
				if (elapsedTimeFromLastInput_ >= completionTimer) {
					StartCompletion();
				}
				break;
			}
			case CompletionState.Complementing: {
				elapsedTimeFromLastInput_ += Time.deltaTime;
				if (elapsedTimeFromLastInput_ > completionTimer + 0.5f /* timeout */) {
					StopCompletion();
				}
				completion_.Update();
				break;
			}
		}

		// update completion view position.
		completionView_.position = inputField_.GetPositionBeforeCaret(currentComletionPrefix_.Length);
	}

	private void StartCompletion()
	{
		if (inputField_.IsNullOrEmpty()) return;

		var code = inputField_.GetStringFromHeadToCaretPosition();
		completion_.Start(code);

		completionState_ = CompletionState.Complementing;
	}

	private void StopCompletion()
	{
		completion_.Stop();
		completionView_.Reset();
		completionState_ = CompletionState.Idle;
	}

	private void OnCompletionFinished(CompletionInfo[] completions)
	{
		if (completions.Length > 0) {
			// TODO: this is not smart... because all items have this information.
			currentComletionPrefix_ = completions[0].prefix;
			completionView_.SetCompletions(completions);
		}
		ResetAnnotation();
		completionState_ = CompletionState.Idle;
	}

	private void DoCompletion()
	{
		// for multiline input
		inputField_.RemoveTabAtCaretPosition();

		var completion = completionView_.selectedCompletion;
		inputField_.InsertToCaretPosition(completion);
		inputField_.MoveCaretPosition(completion.Length);
		inputField_.Select();

		StopCompletion();
	}

	private void RegisterListeners()
	{
		inputField_.onValueChanged.AddListener(OnValueChanged);
		inputField_.onEndEdit.AddListener(OnSubmit);
	}

	private void UnregisterListeners()
	{
		inputField_.onValueChanged.RemoveListener(OnValueChanged);
		inputField_.onEndEdit.RemoveListener(OnSubmit);
	}

	private bool IsEnterPressing()
	{
		if (!inputField_.multiLine) {
			return KeyUtil.Enter();
		} else {
			return (KeyUtil.Control() || KeyUtil.Shift()) && KeyUtil.Enter();
		}
	}

	private void OnValueChanged(string text)
	{
		if (!inputField_.multiLine) {
			text = text.Replace("\n", "");
			text = text.Replace("\r", "");
			inputField_.text = text;
		}

		if (completionState_ != CompletionState.Stop) {
			completionState_ = CompletionState.WaitingForCompletion;
			elapsedTimeFromLastInput_ = 0f;
		}

		RunOnEndOfFrame(completionView_.Reset);
	}

	private void OnSubmit(string code)
	{
		code = code.Trim();

		// do nothing if following states:
		// - the input text is empty.
		// - receive the endEdit event without the enter key (e.g. lost focus).
		if (string.IsNullOrEmpty(code) || !IsEnterPressing()) return;

		// stop completion to avoid hang.
		completion_.Stop();

		// auto-complete semicolon.
		if (!code.EndsWith(";")) {
			code += ";";
		}

		var result = Evaluator.Evaluate(code);
		var item = output_.AddResultItem();

		if (item) {
			switch (result.type) {
				case CompileResult.Type.Success: {
					inputField_.Clear();
					history_.Add(result.code);
					history_.Reset();
					item.type   = CompileResult.Type.Success;
					item.input  = result.code;
					item.output = result.value.ToString();
					break;
				}
				case CompileResult.Type.Partial: {
					// This block should not be reached because the given code is 
					// added a semicolon to end of it. 
					inputField_.Clear();
					item.type   = CompileResult.Type.Partial;
					item.input  = result.code;
					item.output = "The given code is something wrong: " + code;
					break;
				}
				case CompileResult.Type.Error: {
					item.type   = CompileResult.Type.Error;
					item.input  = result.code;
					item.output = result.error;
					break;
				}
			}
		}
	}

	public void OutputLog(Log.Data data)
	{
		// enqueue given datas temporarily to handle data from other threads.
		logData_.Enqueue(data);
	}

	private void UpdateLogs()
	{
		while (logData_.Count > 0) {
			var data = logData_.Dequeue();
			var item = output_.AddLogItem();
			item.level = data.level;
			item.log   = data.log;
			item.meta  = data.meta;
		}
	}

	private void ResetAnnotation()
	{
		elapsedTimeFromLastSelect_ = 0f;
	}

	private void UpdateAnnotation()
	{
		elapsedTimeFromLastSelect_ += Time.deltaTime;

		var item = completionView_.selectedItem;
		var hasDescription = (item != null) && item.hasDescription;
		if (hasDescription) annotation_.text = item.description;

		var isAnnotationVisible = elapsedTimeFromLastSelect_ >= annotationTimer;
		annotation_.gameObject.SetActive(isAnnotationVisible && hasDescription);

		annotation_.transform.position =
			completionView_.selectedPosition + Vector3.right * (completionView_.width + 4f);
	}

	static public GameObject InstantiateInOutputView(GameObject prefab)
	{
		if (selected == null) return null;
		return selected.output_.AddObject(prefab);
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

	public void RunOnNextFrame(System.Action func)
	{
		StartCoroutine(_RunOnNextFrame(func));
	}

	private IEnumerator _RunOnNextFrame(System.Action func)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		func();
	}


	[Command(name = "clear outputs", description = "Clear output view.")]
	static public void ClearOutputCommand()
	{
		if (Gui.selected == null) return;
		Gui.selected.RunOnNextFrame(Gui.selected.output_.Clear);
	}

	[Command(name = "clear histories", description = "Clear all input histories.")]
	static public void ClearHistoryCommand()
	{
		if (Gui.selected == null) return;
		Gui.selected.RunOnNextFrame(Gui.selected.history_.Clear);
	}

	[Command(name = "show histories", description = "show command histoies.")]
	static public void ShowHistory()
	{
		if (Gui.selected == null) return;

		string histories = "";
		int num = Gui.selected.history_.Count;
		foreach (var command in Gui.selected.history_.list.ToArray().Reverse()) {
			histories += string.Format("{0}: {1}\n", num, command);
			--num;
		}
		Log.Output(histories);
	}
}

}