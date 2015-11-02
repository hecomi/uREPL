using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CommandInputField : InputField
{
	public override void OnUpdateSelected(BaseEventData eventData)
	{
		if (!isFocused) return;

		bool consumedEvent = false;
		Event e = new Event();
		while (Event.PopEvent(e)) {
			if (e.rawType == EventType.KeyDown) {
				consumedEvent = true;
				var shouldContinue = KeyPressed(e);
				if (shouldContinue == EditState.Finish) {
					// DeactivateInputField();
					break;
				}
			}

			switch (e.type) {
				case EventType.ValidateCommand:
				case EventType.ExecuteCommand:
					switch (e.commandName) {
						case "SelectAll":
							SelectAll();
							consumedEvent = true;
							break;
					}
					break;
			}
		}

		if (consumedEvent) {
			UpdateLabel();
		}

		eventData.Use();
	}
}
