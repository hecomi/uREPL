using UnityEngine;

namespace uREPL
{

public class OutputView : MonoBehaviour
{
	private Transform content_;
	private GameObject resultItemPrefab_;
	private GameObject logItemPrefab_;

	public int maxResultNum = 100;

	void Awake()
	{
		content_ = transform.Find("Content");

		// Prefabs
		resultItemPrefab_ = Resources.Load<GameObject>("uREPL/Prefabs/Output/Result Item");
		logItemPrefab_    = Resources.Load<GameObject>("uREPL/Prefabs/Output/Log Item");
	}

	public void Clear()
	{
		for (int i = 0; i < content_.childCount; ++i) {
			Destroy(content_.GetChild(i).gameObject);
		}
	}

	public void RemoveExceededItem()
	{
		if (content_.childCount > maxResultNum) {
			Destroy(content_.GetChild(0).gameObject);
		}
	}

	public GameObject AddObject(GameObject prefab)
	{
		var obj = Instantiate(prefab, content_.position, content_.rotation) as GameObject;
		obj.transform.SetParent(content_);
		obj.transform.localScale = Vector3.one;

		RemoveExceededItem();

		return obj;
	}

	public ResultItem AddResultItem()
	{
		return AddObject(resultItemPrefab_).GetComponent<ResultItem>();
	}

	public LogItem AddLogItem()
	{
		return AddObject(logItemPrefab_).GetComponent<LogItem>();
	}
}

}