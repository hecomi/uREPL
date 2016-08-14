using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace uREPL
{

[RequireComponent(typeof(Main))]
public class Window : MonoBehaviour
{
	#region [constant values]
	private const float COMPLETION_TIMEOUT = 0.5f;
	#endregion

	#region [core]
	static public Window selected;
	private bool isWindowOpened_ = false;

	public Main main { get; set; }
	public Parameters parameters 
	{ 
		get { return main.parameters; }
	}

	private Completion completion_ = new Completion();
	public Completion completion
	{
		get { return completion_; }
		private set { completion_ = value; }
	}

	private History history_ = new History();
	public History history
	{
		get { return history_; }
		private set { history_ = value; }
	}

	public enum CompletionState {
		Idle,
		Stop,
		WaitingForCompletion,
		Complementing,
	}
	private CompletionState completionState_ = CompletionState.Idle;
	public CompletionState completionState
	{
		get { return completionState_;  }
		set { completionState_ = value; }
	}
	private string completionPartialCode_ = "";

	private KeyBinding keyBinding_ = new KeyBinding();
	#endregion

	#region [content]
	private CommandView commandView_;
	public CommandView commandView 
	{
		get { return commandView_; }
		private set { commandView_ = value; }
	}

	public CommandInputField inputField
	{
		get { return commandView.inputField; }
		private set { commandView.inputField = value; }
	}

	private OutputView outputView_;
	public OutputView outputView
	{
		get { return outputView_; }
		private set { outputView_ = value; }
	}

	private CompletionView completionView_;
	public CompletionView completionView
	{
		get { return completionView_; }
		private set { completionView_ = value; }
	}

	private float elapsedTimeFromLastInput_ = 0f;
	#endregion

	#region [accessors]
	public bool isOpen
	{
		get { return isWindowOpened_; }
	}
	public bool hasCompletion
	{
		get { return completionView.hasItem; }
	}
	#endregion

	void Awake()
	{
		InitObjects();
		keyBinding_.Initialize(this);

		completion.AddCompletionFinishedListener(OnCompletionFinished);

		isWindowOpened_ = GetComponent<Canvas>().isActiveAndEnabled;
		if (isWindowOpened_) {
			selected = this;
		}
	}

	void InitObjects()
	{
		// Instances
		var container = transform.Find("Container");
		commandView = container.Find("Command View").GetComponent<CommandView>();
		outputView = container.Find("Output View").GetComponent<OutputView>();
		completionView = transform.Find("Completion View").GetComponent<CompletionView>();

		// Settings
		commandView.Initialize(this);
		completionView.Initialize(this);
	}

	void Start()
	{
		RegisterListeners();
		history.Load();
	}

	void OnDestroy()
	{
		completion.Stop();
		UnregisterListeners();
		history.Save();
		completion.RemoveCompletionFinishedListener(OnCompletionFinished);
	}

	void Update()
	{
		keyBinding_.Update();
		UpdateCompletion();
	}

	public void Open()
	{
		selected = this;
		SetActive(true);
		inputField.Focus();
	}

	public void Close()
	{
		if (selected == this) {
			selected = null;
		}
		SetActive(false);
	}

	private void SetActive(bool active)
	{
		GetComponent<Canvas>().enabled = active;
		inputField.gameObject.SetActive(active);
		outputView.gameObject.SetActive(active);
		completionView.gameObject.SetActive(active);
		isWindowOpened_ = active;
	}

	private void UpdateCompletion()
	{
		if (!isWindowOpened_) return;

		switch (completionState) {
			case CompletionState.Idle: {
				break;
			}
			case CompletionState.Stop: {
				completionState = CompletionState.Idle;
				break;
			}
			case CompletionState.WaitingForCompletion: {
				elapsedTimeFromLastInput_ += Time.deltaTime;
				if (elapsedTimeFromLastInput_ >= parameters.completionDelay) {
					StartCompletion();
				}
				break;
			}
			case CompletionState.Complementing: {
				elapsedTimeFromLastInput_ += Time.deltaTime;
				if (elapsedTimeFromLastInput_ > parameters.completionDelay + COMPLETION_TIMEOUT) {
					StopCompletion();
				}
				completion.Update();
				break;
			}
		}

		// update completion view position.
		var completionPosition = inputField.GetPositionBeforeCaret(completionPartialCode_.Length);
		var commandViewPosition = commandView.GetComponent<RectTransform>().localPosition;
		completionView.position = commandViewPosition + completionPosition;
	}

	public void StartCompletion()
	{
		if (inputField.IsNullOrEmpty()) return;

		var code = inputField.GetStringFromHeadToCaretPosition();
		completion.Start(code);

		completionState = CompletionState.Complementing;
	}

	public void StopCompletion()
	{
		completion.Stop();
		completionView.Reset();
		completionState = CompletionState.Idle;
	}

	private void OnCompletionFinished(Completion.Result result)
	{
		completionPartialCode_ = result.partialCode ?? "";
		completionView.SetCompletions(result.completions);
		completionState = CompletionState.Idle;
	}

	public void DoCompletion()
	{
		var completion = completionView.selectedCompletion;
		inputField.InsertToCaretPosition(completion);
		inputField.MoveCaretPosition(completion.Length);
		inputField.Focus();

		StopCompletion();
	}

	private void RegisterListeners()
	{
		inputField.onValueChanged.AddListener(OnValueChanged);
		inputField.onEndEdit.AddListener(OnSubmit);
	}

	private void UnregisterListeners()
	{
		inputField.onValueChanged.RemoveListener(OnValueChanged);
		inputField.onEndEdit.RemoveListener(OnSubmit);
	}

	private void OnValueChanged(string text)
	{
		if (!inputField.multiLine) {
			text = text.Replace("\n", "");
			text = text.Replace("\r", "");
			inputField.text = text;
		}

		if (completionState != CompletionState.Stop) {
			completionState = CompletionState.WaitingForCompletion;
			elapsedTimeFromLastInput_ = 0f;
		}

		Utility.RunOnEndOfFrame(completionView.Reset);
	}

	public void OnSubmit(string code)
	{
		code = code.Trim();

		// do nothing if following states:
		// - the input text is empty.
		// - receive the endEdit event without the enter key (e.g. lost focus).
		if (string.IsNullOrEmpty(code) || !keyBinding_.IsEnterPressing()) return;

		// stop completion to avoid hang.
		completion.Stop();

		var result = Evaluator.Evaluate(code);
		var item = outputView.AddResultItem();

		if (item) {
			switch (result.type) {
				case CompileResult.Type.Success: {
					inputField.Clear();
					history.Add(result.code);
					history.Reset();
					item.type   = CompileResult.Type.Success;
					item.input  = result.code;
					item.output = result.value.ToString();
					break;
				}
				case CompileResult.Type.Partial: {
					// This block should not be reached because the given code is 
					// added a semicolon to end of it. 
					inputField.Clear();
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
		outputView.OutputLog(data);
	}

	static public GameObject InstantiateInOutputView(GameObject prefab)
	{
		if (Window.selected == null) return null;
		return Window.selected.outputView.AddObject(prefab);
	}

	[Command(name = "clear outputs", description = "Clear output view.")]
	static public void ClearOutputCommand()
	{
		if (Window.selected == null) return;
		Utility.RunOnNextFrame(Window.selected.outputView.Clear);
	}

	[Command(name = "clear histories", description = "Clear all input histories.")]
	static public void ClearHistoryCommand()
	{
		if (Window.selected == null) return;
		Utility.RunOnNextFrame(Window.selected.history.Clear);
	}

	[Command(name = "show histories", description = "show command histoies.")]
	static public void ShowHistory()
	{
		if (Window.selected == null) return;

		string histories = "";
		int num = Window.selected.history.Count;
		foreach (var command in Window.selected.history.list.ToArray().Reverse()) {
			histories += string.Format("{0}: {1}\n", num, command);
			--num;
		}
		Log.Output(histories);
	}

	[Command(name = "close", description = "Close console.")]
	static public void CloseCommand()
	{
		Utility.RunOnNextFrame(() => {
			if (Window.selected) Window.selected.Close();
		});
	}
}

}