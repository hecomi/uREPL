using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace uREPL
{

public class ResultItem : MonoBehaviour
{
	private CompileResult.Type type_ = CompileResult.Type.Success;
	public CompileResult.Type type
	{
		get { return type_; }
		set {
			type_ = value;
			image_.color = bgColors[type_];
		}
	}

	private static readonly Dictionary<CompileResult.Type, Color32> bgColors =
		new Dictionary<CompileResult.Type, Color32> {
			{ CompileResult.Type.Success, new Color32( 16,  77,  47, 100) },
			{ CompileResult.Type.Partial, new Color32(100, 100,  17, 100) },
			{ CompileResult.Type.Error,   new Color32(152,  31,  31, 100) },
		};

	private Image image_;
	public Text inputText;
	public Text outputText;

	public string input
	{
		get { return inputText.text;  }
		set { inputText.text = value; }
	}

	public string output
	{
		get { return outputText.text;  }
		set { outputText.text = value; }
	}

	void Awake()
	{
		image_ = GetComponent<Image>();
	}
}

}
