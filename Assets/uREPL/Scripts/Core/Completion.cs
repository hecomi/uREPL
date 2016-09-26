using UnityEngine;
using System.Threading;

namespace uREPL
{

public class Completion
{
	private Thread thread_;
	private bool hasCompletionFinished_ = false;

	public struct Result
	{
		public string partialCode;
		public CompletionInfo[] completions;
	}
	private Result result_;

	public delegate void CompletionFinishHandler(Result result);
	private event CompletionFinishHandler onCompletionFinished_ = result => {};

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
			var completions = CompletionPluginManager.GetCompletions(code);
			result_.completions = completions;
			result_.partialCode = completions[0].prefix ?? "";
		});
		thread_.Start();
	}

	public void Stop()
	{
		if (thread_ != null) {
			if (!thread_.Join(1000)) {
				Debug.LogError("uREPL completion thread stopped unexpectedly...");
				thread_.Abort();
			}
		}
		thread_ = null;
		hasCompletionFinished_ = true;
	}

	public void Update()
	{
		// Even if the completion thread has finished but the finished flag has been set as true,
		// call all handlers to notify them completion results from the main thread.
		if (!IsAlive() && !hasCompletionFinished_) {
			hasCompletionFinished_ = true;
			onCompletionFinished_(result_);
		}
	}
}

}