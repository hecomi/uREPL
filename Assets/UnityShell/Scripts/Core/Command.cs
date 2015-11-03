using UnityEngine;
using System.Collections;

namespace UnityShell
{

static public class Commands
{
	[Command("Close console.", command = "quit")]
	static public void Quit()
	{
		Gui.Close();
	}

	[Command("Open console.", command = "open")]
	static public void Open()
	{
		Gui.Open();
	}
}

}
