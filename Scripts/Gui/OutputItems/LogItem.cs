using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace uREPL
{

public class LogItem : MonoBehaviour
{
	private static readonly Dictionary<Log.Level, Color32> bgColors =
		new Dictionary<Log.Level, Color32> {
			{ Log.Level.Verbose, new Color32( 75,  75,  75, 128) },
			{ Log.Level.Warn,    new Color32(100, 100,  17, 128) },
			{ Log.Level.Error,   new Color32(152,  31,  31, 128) },
		};

	private Image image_;
	public Text logText;
	public Text metaText;

	private Log.Level level_ = Log.Level.Verbose;
	public Log.Level level
	{
		get { return level_; }
		set {
			level_ = value;
			image_.color = bgColors[level_];
		}
	}

	public string log
	{
		get { return logText.text;  }
		set { logText.text = value; }
	}

	public string meta
	{
		get { return metaText.text;  }
		set { metaText.text = value; }
	}

	void Awake()
	{
		image_ = GetComponent<Image>();
	}
}

}
