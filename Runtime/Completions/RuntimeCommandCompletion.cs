using UnityEngine;
using System.Linq;

namespace uREPL
{

public class RuntimeCommandCompletion : CompletionPlugin
{
    public override CompletionInfo[] GetCompletions(string input)
    {
        var table = RuntimeCommands.table;
        return table
            .Where(pair => pair.Key.IndexOf(input) == 0)
            .Select(pair => new CompletionInfo(
                input,
                pair.Key + " ",
                "R",
                new Color32(200, 50, 30, 255),
                string.Format("{0} <color=#888888ff> (Format: <b><i>{1}</i></b></color>)",
                    !string.IsNullOrEmpty(pair.Value.description) ? pair.Value.description : "no description",
                    !string.IsNullOrEmpty(pair.Value.format)      ? pair.Value.format      : "no argument")))
            .ToArray();
    }
}

}
