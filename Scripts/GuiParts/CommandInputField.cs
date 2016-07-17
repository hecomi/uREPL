using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace uREPL
{

public class CommandInputField : InputField
{
	public Gui parentGui { get; set; }

	// To change default behavior and to support multiple gui,
	// override InputFiled method here.
	public override void OnUpdateSelected(BaseEventData eventData)
	{
		if (!isFocused) return;

		// To support multiple gui, set the window that this input filed is belonging to 
		// as the current active window.
		Gui.selected = parentGui;

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
				// Command submission and cancel are observed in Core.
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
}

}
