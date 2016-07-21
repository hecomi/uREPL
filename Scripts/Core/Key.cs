using UnityEngine;

namespace uREPL
{

public static class KeyUtil
{
	static public bool Shift()
	{
		return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
	}

	static public bool Alt()
	{
		return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
	}

	static public bool Control()
	{
		return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
	}

	static public bool Enter()
	{
		return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
	}
}

}