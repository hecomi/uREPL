using UnityEngine;
using System.Linq;

namespace uREPL
{

public class GlobalClassCompletion : CompletionPlugin
{
    static private string[] globalClassNames_;

    protected override void OnEnable()
    {
        if (globalClassNames_ == null) {
            globalClassNames_ = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsClass && type.Namespace == null)
                .Select(type => type.Name)
                .Where(name => char.IsLetter(name[0]))
                .Distinct()
                .OrderBy(name => name[0])
                .ToArray();
        }

        base.OnEnable();
    }

    public override CompletionInfo[] GetCompletions(string input)
    {
        var parts = input.Split(new char[] { '\n', ' ', '\t', '=', '{', '}', '(', ')', '<', '>' });
        var lastPart = parts.Last();
        if (string.IsNullOrEmpty(lastPart)) return null;

        return globalClassNames_
            .Where(name => name.IndexOf(lastPart) == 0)
            .Select(name => new CompletionInfo(
                lastPart,
                name,
                "G",
                new Color32(50, 70, 240, 255),
                "global::" + name))
            .ToArray();
    }
}

}
