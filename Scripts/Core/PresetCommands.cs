using UnityEngine;
using System.Linq;

namespace uREPL
{

public static class PresetCommands
{
	[Command(name = "close", description = "Close console.")]
	static public void CloseCommand()
	{
		Utility.RunOnNextFrame(() => {
			if (Gui.selected) Gui.selected.CloseWindow();
		});
	}
}

}