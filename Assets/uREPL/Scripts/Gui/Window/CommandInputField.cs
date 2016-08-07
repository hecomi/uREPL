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

				// Skip because these keys are used for histroy selection in single-line mode.
				if (!multiLine) {
					switch (e.keyCode) {
						case KeyCode.UpArrow:
						case KeyCode.DownArrow:
							continue;
					}
				}

				// Skip if completion view is open.
				if (parentWindow.hasCompletion) {
					if (e.keyCode == KeyCode.Tab || KeyUtil.Enter()) {
						break;
					}
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

	public void MoveCaretPositionToLineHead()
	{
		if (multiLine) {
			if (caretPosition == 0) return;
			var lastEnd = text.LastIndexOf('\n', caretPosition - 1);
			if (lastEnd != -1) {
				caretPosition = lastEnd + 1;
			} else {
				MoveTextStart(false);
			}
		} else {
			MoveTextStart(false);
		}
	}

	public void MoveCaretPositionToLineEnd()
	{
		if (multiLine) {
			var nextEnd = text.IndexOf('\n', caretPosition);
			if (nextEnd != -1) {
				caretPosition = nextEnd;
			} else {
				MoveTextEnd(false);
			}
		} else {
			MoveTextEnd(false);
		}
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

	public void DeleteCharactersToLineEndAfterCaretPosition()
	{
		if (caretPosition == text.Length) return;

		if (multiLine) {
			var currentLineEnd = text.IndexOf('\n', caretPosition);
			if (currentLineEnd != -1) {
				// to remove the line when the current line is empty.
				var count = Mathf.Max(currentLineEnd - caretPosition, 1);
				text = text.Remove(caretPosition, count);
			} else {
				text = text.Remove(caretPosition);
			}
		} else {
			if (caretPosition < text.Length) {
				text = text.Remove(caretPosition);
			}
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

	public void MoveUp()
	{
		if (!multiLine) return;

		var currentLineHead = text.LastIndexOf('\n', Mathf.Max(caretPosition - 1, 0)) + 1;
		if (currentLineHead != 0) {
			var charCountFromLineHead = caretPosition - currentLineHead;
			var preLineHead = text.LastIndexOf('\n', currentLineHead - 2) + 1;
			var preLineEnd = text.IndexOf('\n', preLineHead);
			caretPosition = Mathf.Min(preLineHead + charCountFromLineHead, preLineEnd);
		}
	}

	public void MoveDown()
	{
		if (!multiLine) return;

		var currentLineHead = text.LastIndexOf('\n', Mathf.Max(caretPosition - 1, 0)) + 1;
		var charCountFromLineHead = caretPosition - currentLineHead;
		var nextLineHead = text.IndexOf('\n', currentLineHead) + 1;
		if (nextLineHead != 0) {
			var nextLineEnd = text.IndexOf('\n', nextLineHead);
			if (nextLineEnd != -1) {
				caretPosition = Mathf.Min(nextLineHead + charCountFromLineHead, nextLineEnd);
			} else {
				MoveTextEnd(false);
			}
		}
	}
}

}