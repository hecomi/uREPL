using UnityEngine;
using UnityEngine.UI;

namespace uREPL
{

public class AnnotationView : MonoBehaviour
{
	public Text description;
	public string text
	{
		get { return description.text;  }
		set { description.text = value; }
	}

	private CompletionView completion_;
}

}
