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
	public Image colorIndicator;

	public Color32 hasDescriptionColor = new Color32(200, 30, 70, 255);
	public Color32 markColor           = Color.gray;
	public Color32 hitTextColor        = Color.white;
	public Color32 bgColor             = Color.black;
	public Color32 highlightedBgColor  = new Color32(30, 30, 30, 200);

	public string description = "";
	public bool hasDescription
	{
		get { return !string.IsNullOrEmpty(description); }
	}

	private string completion_;
	public string completion
	{
		get { return completion_;  }
	}

	private Image image_;

	void Awake()
	{
		image_ = GetComponent<Image>();
	}

	void Update()
	{
		if (hasDescription) {
			colorIndicator.color = hasDescriptionColor;
		}
	}

	public void SetHighlight(bool isHighlighted)
	{
		image_.color = isHighlighted ? highlightedBgColor : bgColor;
	}

	public void SetCompletion(string code, string prefix)
	{
		completion_ = string.IsNullOrEmpty(prefix) ? code : code.Substring(prefix.Length);
		var hitTextColorHex =
			hitTextColor.r.ToString("X2") +
			hitTextColor.g.ToString("X2") +
			hitTextColor.b.ToString("X2") +
			hitTextColor.a.ToString("X2");
		completionText.text = string.Format(
			"<b><color=#{2}>{0}</color></b>{1}",
			prefix,
			completion_,
			hitTextColorHex.ToString());
	}

	public void SetMark(string mark, Color32 color)
	{
		markText.text  = mark;
		markText.color = color;
	}
}

}
