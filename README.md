uREPL
=====

uREPL is an in-game powerful REPL envinronment for Unity3D that supports following functions:

- Execute any Unity-supported C# code at run time.
- Support completions.
- Emacs-like keyboard shortcuts.
- Output various logs.
- Add commands just by adding an attribute.
- Add optional completion plugins easily.


Demo
----
Sorry, there is no demo now...


Environment
-----------
- Mac / Windows
- Unity 5


Installation
------------
Sorry, there is no relase now...


Usage
-----
1. Select menu from `Assets/Create/uREPL` to instantiate a `uREPL` prefab.
2. Set the `Open Key` of the `Gui` component attached to the `uREPL` *GameObject* (default is `F1`).
3. Run game and hit the key.
4. Input code into the input filed, then press Enter key to submit and evaluate it.


Completion
----------
uREPL supports three completions:

- Context
- Command
- *GameObject* name / path


Keybinds
--------

### Input
| Key                       | Description                                  |
| ------------------------- | -------------------------------------------- |
| `ctrl + a`                | move to head.                                |
| `ctrl + e`                | move to end.                                 |
| `ctrl + f`, `right arrow` | move forward.                                |
| `ctrl + b`, `left arrow`  | move back.                                   |
| `ctrl + h`, `backspace`   | remove character before cursor position.     |
| `ctrl + d`                | remove character after cursor position.      |
| `ctrl + k`                | remove all characters after cursor position. |
| `ctrl + l`                | clear all outputs.                           |
| `ctrl + n`, `up arrow`    | show next history.                           |
| `ctrl + p`, `down arrow`  | show previous history.                       |
| `ctrl + tab`              | show complementions.                         |
| `enter`                   | run input command.                           |

### Completion
| Key                       | Description                                  |
| ------------------------- | -------------------------------------------- |
| `ctrl + n`, `up arrow`    | select lower item.                           |
| `ctrl + p`, `down arrow`  | select upper item.                           |
| `tab`, `enter`            | insert selected completion.                  |
| `esc`                     | hide complementions.                         |


Logs
----
You can output 3 level logs by `uREPL.Log.Output(string)`, `uREPL.Log.Warn(string)`,
and `uREPL.Log.Error(string)`.

```cs
public class LogTest
{
	void ShowLogs()
	{
		uREPL.Log.Output("this is normal log.");
		uREPL.Log.Warn("this is warning log.");
		uREPL.Log.Error("this is error log.");
	}
}
```


Commands
--------
You can add commands by adding a `[uREPL.Command]` attribute to static methods.

```cs
public class CommandTest
{
	// Given from somewhere.
	public GameObject gameObject;

	// This method can be called without class name.
	// $ ShowCurrentSelectedObject() ⏎
	[uREPL.Command]
	static public void ShowCurrentSelectedObject()
	{
		return gameObject.Name;
	}

	// This method can be called by the given name.
	// $ selected ⏎
	[uREPL.Command]
	[uREPL.Command(name = "selected")]
	static public void ShowCurrentSelectedObject2()
	{
		return gameObject.Name;
	}

	// Completion view show the given description.
	[uREPL.Command(name = "selected2", description = "show the selected gameobject name.")]
	static public void ShowCurrentSelectedObject3()
	{
		return gameObject.Name;
	}
}
```


Add completion plugin
---------------------
You can add completion plugins by adding a class that inherits from `uREPL.CompletionPlugin` and overrides `GetCompletions()`.
This class is derived from `MonoBehaviour`, so you can collect information using its callbacks.
The following code is a sample for *GameObject* name completion.

```cs
using UnityEngine;
using System.Linq;
using uREPL;

public class SampleCompletion : CompletionPlugin
{
	string[] gameObjectNames_;

	public void Update()
	{
		gameObjectNames_ = GameObject.FindObjectsOfType<GameObject>()
			.Select(go => go.name)
			.ToArray();
	}

	// This method is called from a non-main thread.
	// If you want to use such as GameObject-related data, please get it in the main thread.
	public override CompletionInfo[] GetCompletions(string input)
	{
		var partialName = input.Substring(input.LastIndexOf("\"") + 1);
		return gameObjectNames_
			.Where(name => name.IndexOf(partialName) != -1)
			.Select(name => new CompletionInfo(
				partialName, name, "G", Color.red))
			.ToArray();
	}
}
```


Others
------
- World Space GUI
  - Set the `Render Mode` of the `Canvas` component as `World Space`.
- Multiple GUI
  - You can locate multiple GUIs in a scene.
- Builds
  - Almost all functions are available even for builds.


TODOs
-----
- Command with arguments.
- Multiline input.
- VR support.
- Various debug mode.
- Various useful commands.
- More log types (e.g. image, graph).
- Show MonoBehaviour parameters and edit them in the log.
- Add statistics log.
- Syntax Highlight

Please request new features to issue.


Version
-------
| Data       | Version | Description    |
| ---------- | ------- | -------------- |
| 2015/11/01 |  0.0.1  | Start project. |
