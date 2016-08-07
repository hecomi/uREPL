using UnityEngine;

namespace uREPL
{

public class KeyBinding
{
	private Window window_ = null;

	private CommandView commandView
	{
		get { return window_.commandView; }
	}

	private CommandInputField inputField
	{
		get { return window_.inputField; }
	}

	private bool isMultiline
	{
		get { return inputField.multiLine; }
	}

	private CompletionView completionView
	{
		get { return window_.completionView; }
	}

	private Window.CompletionState completionState
	{
		get { return window_.completionState; }
		set { window_.completionState = value; }
	}

	private History history
	{
		get { return window_.history; }
	}

	private KeyEvent keyEvent_ = new KeyEvent();
	[SerializeField] KeyCode openKey = KeyCode.F1;
	[SerializeField] KeyCode closeKey = KeyCode.F1;

	public void Initialize(Window parent)
	{
		window_ = parent;
		InitializeArrowKeys();
		InitializeCommands();
		InitializeEmacsLikeKeys();
	}

	private void InitializeArrowKeys()
	{
		keyEvent_.Add(KeyCode.UpArrow, Prev);
		keyEvent_.Add(KeyCode.DownArrow, Next);
		keyEvent_.Add(KeyCode.LeftArrow, StopCompletion);
		keyEvent_.Add(KeyCode.RightArrow, StopCompletion);
	}

	private void InitializeCommands()
	{
		keyEvent_.Add(KeyCode.Escape, StopCompletion);
		keyEvent_.Add(KeyCode.Tab, () => {
			if (completionView.hasItem) {
				window_.DoCompletion();
			} else {
				window_.StartCompletion();
			}
		});
		keyEvent_.Add(KeyCode.M, KeyEvent.Option.Ctrl, commandView.ToggleLineType);
	}

	private void InitializeEmacsLikeKeys()
	{
		keyEvent_.Add(KeyCode.P, KeyEvent.Option.Ctrl, Prev);
		keyEvent_.Add(KeyCode.N, KeyEvent.Option.Ctrl, Next);
		keyEvent_.Add(KeyCode.F, KeyEvent.Option.Ctrl, () => {
			inputField.MoveCaretPosition(1);
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.B, KeyEvent.Option.Ctrl, () => {
			inputField.MoveCaretPosition(-1);
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.A, KeyEvent.Option.Ctrl, () => {
			inputField.MoveCaretPositionToLineHead();
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.E, KeyEvent.Option.Ctrl, () => {
			inputField.MoveCaretPositionToLineEnd();
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.H, KeyEvent.Option.Ctrl, () => {
			inputField.BackspaceOneCharacterFromCaretPosition();
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.D, KeyEvent.Option.Ctrl, () => {
			inputField.DeleteOneCharacterFromCaretPosition();
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.K, KeyEvent.Option.Ctrl, () => {
			inputField.DeleteCharactersToLineEndAfterCaretPosition();
			StopCompletion();
		});
		keyEvent_.Add(KeyCode.L, KeyEvent.Option.Ctrl, window_.outputView.Clear);
	}

	private void ToggleWindowByKeys()
	{
		if (openKey == closeKey) {
			if (Input.GetKeyDown(openKey)) {
				if (!window_.isOpen) window_.Open();
				else window_.Close();
			}
		} else {
			if (Input.GetKeyDown(openKey)) window_.Open();
			if (Input.GetKeyDown(closeKey)) window_.Close();
		}
	}

	public bool IsEnterPressing()
	{
		if (!inputField.multiLine) {
			return KeyUtil.Enter();
		} else {
			return (KeyUtil.Control() || KeyUtil.Shift()) && KeyUtil.Enter();
		}
	}

	private void Prev()
	{
		if (completionView.hasItem) {
			completionView.Next();
		} else if (isMultiline) {
			inputField.MoveUp();
		} else {
			if (history.IsFirst()) history.SetInputtingCommand(inputField.text);
			inputField.text = history.Prev();
			inputField.MoveTextEnd(false);
			completionState = Window.CompletionState.Stop;
		}
	}

	private void Next()
	{
		if (completionView.hasItem) {
			completionView.Prev();
		} else if (isMultiline) {
			inputField.MoveDown();
		} else {
 			inputField.text = history.Next();
			inputField.MoveTextEnd(false);
			completionState = Window.CompletionState.Stop;
		}
	}

	private void StopCompletion()
	{
		window_.StopCompletion();
	}

	public void Update()
	{
		ToggleWindowByKeys();

		if (window_.isOpen) {
			if (inputField.isFocused) {
				keyEvent_.Check();
			} else {
				keyEvent_.Clear();
			}

			if (window_.hasCompletion && KeyUtil.Enter()) {
				window_.DoCompletion();
			} else if (IsEnterPressing()) {
				window_.OnSubmit(inputField.text);
			}
		}
	}
}

}