using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace uREPL
{

public class History
{
	private const string userPrefsKey = "unityshell-history";
	private const string separator = "\r\n";
	private const int maxNum = 100;
	private List<string> list_ = new List<string>();
	private int index_ = -1;
	private string inputtingCommand_ = "";

	public int Count
	{
		get { return list_.Count; }
	}

	public List<string> list
	{
		get { return list_; }
	}

	public bool IsFirst()
	{
		return index_ == -1;
	}

	public string Get()
	{
		return index_ == -1 ? inputtingCommand_ : list_[index_];
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

	public void SetInputtingCommand(string code)
	{
		inputtingCommand_ = code;
	}

	public void Reset()
	{
		index_ = -1;
		inputtingCommand_ = "";
	}

	public void Add(string code)
	{
		if (Count > 0 && list_[Count - 1] == code) return;

		int index = list_.IndexOf(code);
		if (index == -1) {
			list_.Insert(0, code);
		} else {
			list_.RemoveAt(index);
			list_.Insert(0, code);
		}

		while (Count > maxNum) {
			list_.RemoveAt(Count - 1);
		}
	}

	public void Clear()
	{
		PlayerPrefs.DeleteKey(userPrefsKey);
		list_.Clear();
	}

	public void Save()
	{
		if (Count > 0) {
			var str = list_.Aggregate((a, b) => a + separator + b);
			PlayerPrefs.SetString(userPrefsKey, str);
		}
	}

	public void Load()
	{
		var str = PlayerPrefs.GetString(userPrefsKey);
		if (!string.IsNullOrEmpty(str)) {
			list_ = str.Split(
				new string[] { separator },
				System.StringSplitOptions.RemoveEmptyEntries).ToList();
		}
	}
}

}
