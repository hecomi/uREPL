using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace UnityShell
{

public class CompletionItem : MonoBehaviour
{
	[System.Serializable]
	public struct Mark
	{
		public CompletionType type;
		public string text;
		public Color32 color;

		public Mark(CompletionType type, string text, Color32 color)
		{
			this.type  = type;
			this.text  = text;
			this.color = color;
		}
	}

	private static readonly List<Mark> marks_ = new List<Mark>() {
		new Mark(CompletionType.Mono,    "M", new Color32(  0, 255,   0, 255)),
		new Mark(CompletionType.Command, "C", new Color32(255,   0,   0, 255)),
		new Mark(CompletionType.Path,    "P", new Color32(  0,   0, 255, 255)),
	};

	public Text markText;
	public Text completionText;

	public Color32 hitTextColor       = Color.white;
	public Color32 bgColor            = Color.black;
	public Color32 highlightedBgColor = new Color32(30, 30, 30, 200);

	private CompletionType type_;
	public CompletionType type
	{
		get { return type_;  }
		set {
			type_ = value;
			var info = marks_.Find(x => x.type == value);
			markText.text  = info.text;
			markText.color = info.color;
		}
	}

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

	public void SetCode(string code, string prefix)
	{
		code_ = code;
		string hitTextColorHex =
			hitTextColor.r.ToString("X2") +
			hitTextColor.g.ToString("X2") +
			hitTextColor.b.ToString("X2") +
			hitTextColor.a.ToString("X2");
		completionText.text = string.Format(
			"<b><color=#{2}>{0}</color></b>{1}", prefix, code, hitTextColorHex.ToString());
	}
}

}
