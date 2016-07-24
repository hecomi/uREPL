using UnityEngine;
using System.Linq;

namespace uREPL
{

public static class PresetCommands
{
	[Command(name = "close", description = "Close console.")]
	static public void CloseCommand()
	{
		if (Gui.selected) {
			Gui.selected.RunOnNextFrame(() => {
				if (Gui.selected) Gui.selected.CloseWindow();
			});
		}
	}

	[Command(name = "clear outputs", description = "Clear output view.")]
	static public void ClearOutputCommand()
	{
		if (Gui.selected == null) return;

		var target = Gui.selected;
		Gui.selected.RunOnNextFrame(target.ClearOutputView);
	}

	[Command(name = "clear histories", description = "Clear all input histories.")]
	static public void ClearHistoryCommand()
	{
		if (Gui.selected == null) return;

		var target = Gui.selected;
		Gui.selected.RunOnNextFrame(target.history.Clear);
	}

	[Command(name = "show histories", description = "show command histoies.")]
	static public void ShowHistory()
	{
		if (Gui.selected == null) return;

		string histories = "";
		int num = Gui.selected.history.Count;
		foreach (var command in Gui.selected.history.list.ToArray().Reverse()) {
			histories += string.Format("{0}: {1}\n", num, command);
			--num;
		}
		Log.Output(histories);
	}
}

}