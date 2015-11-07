using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace uREPL
{

public class CompletionItem : MonoBehaviour
{
	public Text markText;
	public Text completionText;

	public Color32 markColor          = Color.gray;
	public Color32 hitTextColor       = Color.white;
	public Color32 bgColor            = Color.black;
	public Color32 highlightedBgColor = new Color32(30, 30, 30, 200);

	private string code_;
	public string code
	{
		get { return code_;  }
	}

	private Image image_;

	void Awake()
	{
		image_ = GetComponent<Image>();
	}

	public void SetHighlight(bool isHighlighted)
	{
		image_.color = isHighlighted ? highlightedBgColor : bgColor;
	}

	public void SetCompletion(string code, string prefix)
	{
		code_ = code;
		var hitTextColorHex =
			hitTextColor.r.ToString("X2") +
			hitTextColor.g.ToString("X2") +
			hitTextColor.b.ToString("X2") +
			hitTextColor.a.ToString("X2");
		completionText.text = string.Format(
			"<b><color=#{2}>{0}</color></b>{1}", prefix, code, hitTextColorHex.ToString());
	}

	public void SetMark(string mark, Color32 color)
	{
		markText.text  = mark;
		markText.color = color;
	}
}

}
