using UnityEngine.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace uREPL
{

public class Completion
{
	private CancellationTokenSource cancellationTokenSource_ = null;

	public struct Result
	{
		public string partialCode;
		public CompletionInfo[] completions;
	}

    public class CompletionFinishEvent : UnityEvent<Result> {};
    public CompletionFinishEvent onCompletionFinished { get; private set; } = new CompletionFinishEvent();

	public async void Start(string code)
	{
		Stop();

		cancellationTokenSource_ = new CancellationTokenSource();
		var token = cancellationTokenSource_.Token;

		var task = Task.Run(() => {
			var completions = CompletionPluginManager.GetCompletions(code);

			token.ThrowIfCancellationRequested();

            var result = new Result();
			result.completions = completions;
			result.partialCode = 
				(completions.Length == 0) ? "" : 
				(completions[0].prefix ?? "");
            return result;
		}, token);

		try
		{
			var result = await task;
			onCompletionFinished.Invoke(result);
		}
		catch (OperationCanceledException)
		{
            // ...
		}
		finally
		{
			if (cancellationTokenSource_ != null) {
				cancellationTokenSource_.Dispose();
				cancellationTokenSource_ = null;
			}
		}
	}

	public void Stop()
	{
		if (cancellationTokenSource_ != null) {
			cancellationTokenSource_.Cancel();
		}
	}
}

}