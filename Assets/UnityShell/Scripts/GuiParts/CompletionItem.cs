using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace UnityShell
{

public class CompletionItem : MonoBehaviour
{
	[System.Serializable]
	public struct Prefix
	{
		public enum Type
		{
			Field    = 0,
			Method   = 1,
			Property = 2,
			Local    = 3
		}
		public string text;
		public Type type;
		public Color32 color;

		public Prefix(Type type, string text, Color32 color)
		{
			this.type  = type;
			this.text  = text;
			this.color = color;
		}
	}

	public List<Prefix> prefixes = new List<Prefix>() {
		new Prefix(Prefix.Type.Field,    "F", new Color32(  0,   0, 255, 255)),
		new Prefix(Prefix.Type.Method,   "M", new Color32(255,   0,   0, 255)),
		new Prefix(Prefix.Type.Property, "P", new Color32(  0, 255,   0, 255)),
		new Prefix(Prefix.Type.Local,    "L", new Color32(128, 128, 128, 255)),
	};

	public Text prefixText;
	public Text completionText;

	public Color32 hitTextColor       = Color.white;
	public Color32 bgColor            = Color.black;
	public Color32 highlightedBgColor = new Color32(30, 30, 30, 200);

	private Prefix.Type prefix_;
	public Prefix.Type prefix
	{
		get { return prefix_;  }
		set {
			prefix_ = value;
			var info = prefixes.Find(x => x.type == value);
			prefixText.text  = info.text;
			prefixText.color = info.color;
		}
	}

	public string code_;
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
