using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace uREPL
{

public class History
{
	private const string key = "unityshell-history";
	private const string separator = "\r\n";
	private const int maxNum = 100;

	private List<string> codes_ = new List<string>();
	private int index_ = -1;
	private string inputting_ = "";

	public int Count
	{
		get { return codes_.Count; }
	}

	public bool IsFirst()
	{
		return index_ == -1;
	}

	public string Get()
	{
		return index_ == -1 ? inputting_ : codes_[index_];
	}

	public string Next()
	{
		if (index_ > -1) --index_;
		return Get();
	}

	public string Prev()
	{
		if (index_ < Count - 1) ++index_;
		return Get();
	}

	public void SetInputting(string code)
	{
		inputting_ = code;
	}

	public void Reset()
	{
		index_ = -1;
		inputting_ = "";
	}

	public void Add(string code)
	{
		if (Count > 0 && codes_[Count - 1] == code) return;
		codes_.Insert(0, code);
		while (Count > maxNum) {
			codes_.RemoveAt(Count - 1);
		}
	}

	public void Clear()
	{
		PlayerPrefs.DeleteKey(key);
		codes_.Clear();
	}

	public void Save()
	{
		if (Count > 0) {
			var str = codes_.Aggregate((a, b) => a + separator + b);
			PlayerPrefs.SetString(key, str);
		}
	}

	public void Load()
	{
		var str = PlayerPrefs.GetString(key);
		if (!string.IsNullOrEmpty(str)) {
			codes_ = str.Split(
				new string[] { separator },
				System.StringSplitOptions.RemoveEmptyEntries).ToList();
		}
	}
}

}
