using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

namespace UnityShell
{

public class CompletionView : MonoBehaviour
{
	private const string contentGameObjectName = "Content";
	private const float scrollDamping = 0.3f;

	public GameObject itemPrefab;
	public Vector3 offset = new Vector3(0, 10, 0);
	private Transform content;
	private ScrollRect scroll;
	private RectTransform rect;

	private int currentIndex_ = 0;

	public int itemCount
	{
		get { return content.childCount; }
	}

	private int itemIndex
	{
		get { return itemCount - 1 - currentIndex_; }
	}

	public Vector3 position
	{
		get { return rect.localPosition; }
		set { rect.localPosition = value + offset; }
	}

	public string selectedCode
	{
		get {
			return itemCount > 0 ?
				content.GetChild(itemIndex).GetComponent<CompletionItem>().code : "";
		}
	}

	void Awake()
	{
		rect    = GetComponent<RectTransform>();
		scroll  = GetComponent<ScrollRect>();
		content = transform.FindChild(contentGameObjectName);
	}

	void Update()
	{
		var targetScrollPos = (itemCount <= 1) ? 0f : 1f * currentIndex_ / (itemCount - 1);
		var scrollPos = scroll.verticalNormalizedPosition;
		scrollPos += (targetScrollPos - scrollPos) * scrollDamping;
		scroll.verticalNormalizedPosition = scrollPos;

		for (int i = 0; i < itemCount; ++i) {
			content.GetChild(i).GetComponent<CompletionItem>().SetHighlight(i == itemIndex);
		}
	}

	public void UpdateCompletion(string[] completions, string prefix)
	{
		Reset();
		if (completions.Length == 0) return;
		foreach (var completion in completions.Reverse()) {
			var itemObject = Instantiate(itemPrefab) as GameObject;
			itemObject.transform.SetParent(content);
			itemObject.GetComponent<CompletionItem>().SetCode(completion, prefix);
		}
	}

	void RemoveAllChildren()
	{
		for (int i = 0; i < itemCount; ++i) {
			var item = content.GetChild(i).gameObject;
			Destroy(item);
		}
	}

	public void Next()
	{
		if (itemCount == 0) {
			currentIndex_ = 0;
		} else {
			currentIndex_ = (currentIndex_ + 1) % itemCount;
		}
	}

	public void Prev()
	{
		if (itemCount == 0) {
			currentIndex_ = 0;
		} else {
			--currentIndex_;
			if (currentIndex_ < 0) {
				currentIndex_ += itemCount;
			}
		}
	}

	public void Reset()
	{
		RemoveAllChildren();
		currentIndex_ = 0;
		scroll.verticalNormalizedPosition = 0f;
	}
}

}
