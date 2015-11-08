uREPL
=====

uREPL is an in-game powerful REPL envinronment for Unity3D that supports following functions:

- Execute any Unity-supported C# code at run time.
- Support completions.
- Emacs-like keyboard shortcuts.
- Output logs.
- Add commands just by adding an attribute.
- Add optional completion methods easily.

This asset helps you debug your projects and learn Unity APIs.


Demo
----
Sorry, there is no demo now...


Installation
------------
Sorry, there is no relase now...


Usage
-----
1. Select menu from `Assets/Create/uREPL` to instantiate a `uREPL` prefab.
2. Set the `Open Key` of the `Gui` component attached to the `uREPL` gameobject (default is `F1`).
3. Run game and hit the key.
4. Input code into the input filed, then press Enter key to submit and evaluate it.


Completion
----------
uREPL supports three completions:

- context
- command
- gameobject path


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
| `up arrow`                | select lower item.                           |
| `down arrow`              | select upper item.                           |
| `ctrl + n`, `up arrow`    | select lower item.                           |
| `ctrl + p`, `down arrow`  | select upper item.                           |
| `tab`                     | insert selected completion.                  |
| `ctrl + esc`              | hide complementions.                         |
| `enter`                   | insert selected completion.                  |


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
	public GameObject gameObject;

	// This method can be called without class name.
	// $ ShowCurrentSelectedObject() [Enter]
	[uREPL.Command]
	static public void ShowCurrentSelectedObject()
	{
		return gameObject.Name;
	}

	// This method can be called by the given name.
	// $ selected [Enter]
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
You can add completion plugins by adding a class that inherits from `CompletionPlugin` and overrides `GetCompletions()`.
This class is derived from `MonoBehaviour`, so you have to attach this script to a certain gameobject.


Others
-----------
- Support World Space GUI.
  - Set the `Render Mode` of the `Canvas` component as `World Space`.


TODOs
-----
- VR support.
- Add useful commands as plugins.
- Add more types of logs (e.g. image, graph).
- Show MonoBehaviour parameters and edit them in the log.
- Add statistics log.


Version
-------
| Data       |  Description    |
| ---------- | --------------- |
| 2015/11/01 |  Start project. |
