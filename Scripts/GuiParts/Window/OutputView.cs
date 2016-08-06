using UnityEngine;
using System.Collections.Generic;

namespace uREPL
{

public class OutputView : MonoBehaviour
{
	private Transform content_;
	private GameObject resultItemPrefab_;
	private GameObject logItemPrefab_;

	public Queue<Log.Data> logData_ = new Queue<Log.Data>();

	public int maxResultNum = 100;

	void Awake()
	{
		content_ = transform.Find("Content");

		// Prefabs
		resultItemPrefab_ = Resources.Load<GameObject>("uREPL/Prefabs/Output/Result Item");
		logItemPrefab_    = Resources.Load<GameObject>("uREPL/Prefabs/Output/Log Item");
	}

	void Update()
	{
		UpdateLogs();
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

	private void UpdateLogs()
	{
		while (logData_.Count > 0) {
			var data = logData_.Dequeue();
			var item = AddLogItem();
			item.level = data.level;
			item.log   = data.log;
			item.meta  = data.meta;
		}
	}

	public void OutputLog(Log.Data data)
	{
		// enqueue given datas temporarily to handle data from other threads.
		logData_.Enqueue(data);
	}
}

}