using UnityEngine;
using System.Linq;

namespace uREPL
{

public class CommandCompletion : CompletionPlugin
{
	static public CommandCompletion instance;
	private CommandInfo[] commands_;

	protected override void OnEnable()
	{
		instance = this;
		commands_ = Commands.GetAll();

		base.OnEnable();
	}

	public override CompletionInfo[] GetCompletions(string input)
	{
		return (commands_ == null) ?
			null :
			commands_
				.Where(x => x.command.IndexOf(input) == 0)
				.Select(x => new CompletionInfo(
					input,
					x.command + " ",
					"C",
					new Color32(200, 50, 30, 255),
					string.Format("{0} <color=#888888ff>- <i>{1}</i></color>",
						x.description,
						x.GetTaggedFormat())))
				.ToArray();
	}
}

}
