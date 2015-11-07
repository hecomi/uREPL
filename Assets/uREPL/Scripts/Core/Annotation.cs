using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class Annotation : MonoBehaviour
{
	public Text description;
	public string text
	{
		get { return description.text;  }
		set { description.text = value; }
	}
}

}
