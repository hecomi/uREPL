using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace uREPL
{

public class CommandInputField : InputField
{
	public Window parentWindow { get; set; }

	// To change default behavior and to support multiple gui,
	// override InputFiled method here.
	public override void OnUpdateSelected(BaseEventData eventData)
	{
		if (!isFocused) return;

		// To support multiple gui, set the window that this input filed is belonging to 
		// as the current active window.
		Window.selected = parentWindow;

		bool consumedEvent = false;
		var e = new Event();
		while (Event.PopEvent(e)) {
			if (e.rawType == EventType.KeyDown) {
				consumedEvent = true;

				// Skip because these keys are used for histroy selection.
				switch (e.keyCode) {
					case KeyCode.UpArrow:
					case KeyCode.DownArrow:
						continue;
				}

				var shouldContinue = KeyPressed(e);

				// Prevent finish.
				// Command submission and cancel are observed and handled in Core.
				switch (e.keyCode) {
					case KeyCode.Return:
					case KeyCode.KeypadEnter:
					case KeyCode.Escape:
						shouldContinue = EditState.Continue;
						break;
				}

				if (shouldContinue == EditState.Finish) {
					DeactivateInputField();
					break;
				}
			}
		}

		if (consumedEvent) {
			UpdateLabel();
		}

		eventData.Use();
	}

	public void MoveCaretPosition(int x)
	{
		caretPosition = Mathf.Clamp(caretPosition + x, 0, text.Length);
	}

	public void BackspaceOneCharacterFromCaretPosition()
	{
		if (caretPosition > 0) {
			var isCaretPositionLast = caretPosition == text.Length;
			text = text.Remove(caretPosition - 1, 1);
			if (!isCaretPositionLast) {
				--caretPosition;
			}
		}
	}

	public void DeleteOneCharacterFromCaretPosition()
	{
		if (caretPosition < text.Length) {
			text = text.Remove(caretPosition, 1);
		}
	}

	public void DeleteAllCharactersAfterCaretPosition()
	{
		if (caretPosition < text.Length) {
			text = text.Remove(caretPosition);
		}
	}

	public bool IsNullOrEmpty()
	{
		return string.IsNullOrEmpty(text);
	}

	public string GetStringFromHeadToCaretPosition()
	{
		return text.Substring(0, caretPosition);
	}

	public void InsertToCaretPosition(string str)
	{
		text = text.Insert(caretPosition, str);
	}

	public void RemoveTabAtCaretPosition()
	{
		// TODO: see the caret position instead of the end of the text.
		if (text.EndsWith("\t")) {
			text = text.Remove(text.Length - 1, 1);
		}
	}

	public Vector3 GetPositionBeforeCaret(int offsetLen)
	{
		if (isFocused) {
			var generator = textComponent.cachedTextGenerator;
			if (caretPosition < generator.characters.Count) {
				var len = caretPosition;
				var info = generator.characters[len];
				var ppu  = textComponent.pixelsPerUnit;
				var x = info.cursorPos.x / ppu;
				var y = info.cursorPos.y / ppu;
				var z = 0f;
				var prefixWidth = 0f;
				for (int i = 0; i < offsetLen && i < len; ++i) {
					prefixWidth += generator.characters[len - 1 - i].charWidth;
				}
				prefixWidth /= ppu;
				var inputTform = GetComponent<RectTransform>();
				return inputTform.localPosition + new Vector3(x - prefixWidth, y, z);
			}
		}
		return -9999f * Vector3.one;
	}

	public void Clear()
	{
		text = "";
	}

	public void Focus()
	{
		ActivateInputField();
		Select();
	}
}

}
