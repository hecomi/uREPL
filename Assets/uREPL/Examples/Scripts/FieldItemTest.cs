using UnityEngine;
using System.Collections.Generic;

namespace uREPL
{

public class FieldItemTest : MonoBehaviour
{
	public enum Number
	{
		One,
		Two,
		Three,
	}

	public bool flag;
	public int counter;
	public float time;
	public string text;
	public Vector2 vec2;
	public Vector3 vec3;
	public Vector4 vec4;
	public Quaternion rot;
	public KeyCode keyCode = KeyCode.Escape;
	public Number number = Number.One;
	public Color32 color = Color.red;
	public List<int> list = new List<int>();

	void Update()
	{
		// dummy
	}
}

}
