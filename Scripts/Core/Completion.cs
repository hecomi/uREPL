using UnityEngine;
using System.Threading;

namespace uREPL
{

public class Completion
{
	private Thread thread_;
	private CompletionInfo[] completions_;

	private string partialCodeForCompletion_;
	private bool hasCompletionFinished_ = false;

	public delegate void CompletionFinishHandler(CompletionInfo[] completions);
	private event CompletionFinishHandler onCompletionFinished_ = completions => {};

	public bool IsAlive()
	{
		return thread_ != null && thread_.IsAlive;
	}

	public void AddCompletionFinishedListener(CompletionFinishHandler callback)
	{
		onCompletionFinished_ += callback;
	}

	public void RemoveCompletionFinishedListener(CompletionFinishHandler callback)
	{
		onCompletionFinished_ -= callback;
	}

	public void Start(string code)
	{
		Stop();
		hasCompletionFinished_ = false;
		thread_ = new Thread(() => {
			completions_ = CompletionPluginManager.GetCompletions(code);
		});
		thread_.Start();
	}

	public void Stop()
	{
		if (thread_ != null) {
			thread_.Abort();
			hasCompletionFinished_ = true;
		}
	}

	public void Update()
	{
		// Even if the completion thread has finished but the finished flag has been set as true,
		// call all handlers to notify them completion results from the main thread.
		if (!IsAlive() && !hasCompletionFinished_) {
			hasCompletionFinished_ = true;
			onCompletionFinished_(completions_);
		}
	}
}

}