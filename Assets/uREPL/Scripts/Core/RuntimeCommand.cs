using System.Collections.Generic;
using System.Linq;

namespace uREPL
{

static public class RuntimeCommands
{
    public class Func
    {
        public string name;
        public System.Action func0 = null;
        public System.Action<object> func1 = null;
        public System.Action<object, object> func2 = null;
        public System.Action<object, object, object> func3 = null;
        public System.Type arg0Type = typeof(object);
        public System.Type arg1Type = typeof(object);
        public System.Type arg2Type = typeof(object);
        public string description = "";

        public string format
        {
            get
            {
                string format = name;
                if      (func1 != null) format += string.Format("  {0}", arg0Type.Name);
                else if (func2 != null) format += string.Format("  {0}  {1}", arg0Type.Name, arg1Type.Name);
                else if (func3 != null) format += string.Format("  {0}  {1}  {2}", arg0Type.Name, arg1Type.Name, arg2Type.Name);
                return format;
            }
        }
    }

    static private Dictionary<string, Func> table_ = new Dictionary<string, Func>();
    static public Dictionary<string, Func> table { get { return table_; } }

    static private bool CheckRegistration(string name)
    {
        if (HasRegistered(name))
        {
            Log.Error(string.Format("Runtime command {0} has been already registered.", name));
            return true;
        }
        return false;
    }

    static public void Register(
        string name,
        System.Action func,
        string description = "")
    {
        if (CheckRegistration(name)) return;
        table_.Add(name, new Func()
        {
            name = name,
            func0 = func,
            func1 = null,
            func2 = null,
            func3 = null,
            description = description,
        });
    }

    static public void Register(
        string name,
        System.Action<object> func,
        string description = "")
    {
        if (CheckRegistration(name)) return;
        table_.Add(name, new Func()
        {
            name = name,
            func0 = null,
            func1 = func,
            func2 = null,
            func3 = null,
            description = description,
        });
    }

    static public void Register<Arg0>(
        string name,
        System.Action<Arg0> func,
        string description = "")
    {
        if (CheckRegistration(name)) return;
        table_.Add(name, new Func()
        {
            name = name,
            func0 = null,
            func1 = x => func((Arg0)x),
            func2 = null,
            func3 = null,
            arg0Type = typeof(Arg0),
            description = description,
        });
    }

    static public void Register(
        string name,
        System.Action<object, object> func,
        string description = "")
    {
        if (CheckRegistration(name)) return;
        table_.Add(name, new Func()
        {
            name = name,
            func0 = null,
            func1 = null,
            func2 = func,
            func3 = null,
            description = description,
        });
    }

    static public void Register<Arg0, Arg1>(
        string name,
        System.Action<Arg0, Arg1> func,
        string description = "")
    {
        if (CheckRegistration(name)) return;
        table_.Add(name, new Func()
        {
            name = name,
            func0 = null,
            func1 = null,
            func2 = (x, y) => func((Arg0)x, (Arg1)y),
            func3 = null,
            arg0Type = typeof(Arg0),
            arg1Type = typeof(Arg1),
            description = description,
        });
    }

    static public void Register(
        string name,
        System.Action<object, object, object> func,
        string description = "")
    {
        if (CheckRegistration(name)) return;
        table_.Add(name, new Func()
        {
            name = name,
            func0 = null,
            func1 = null,
            func2 = null,
            func3 = func,
            description = description,
        });
    }

    static public void Register<Arg0, Arg1, Arg2>(
        string name,
        System.Action<Arg0, Arg1, Arg2> func,
        string description = "")
    {
        if (CheckRegistration(name)) return;
        table_.Add(name, new Func()
        {
            name = name,
            func0 = null,
            func1 = null,
            func2 = null,
            func3 = (x, y, z) => func((Arg0)x, (Arg1)y, (Arg2)z),
            arg0Type = typeof(Arg0),
            arg1Type = typeof(Arg1),
            arg2Type = typeof(Arg2),
            description = description,
        });
    }

    static public void Unregister(string name)
    {
        table_.Remove(name);
    }

    static public bool HasRegistered(string name)
    {
        return table_.ContainsKey(name);
    }

    static private void OutputFormatError(Func func)
    {
        if (func == null) {
            Log.Error("Runtime command parser seems to have unknown error.");
            return;
        }

        string error = string.Format("Runtime command \"{0}\" must have ", func.name);
        if      (func.func1 != null) error += "one argument.";
        else if (func.func2 != null) error += "two arguments.";
        else if (func.func3 != null) error += "three arguments.";
        else                         error += "no argument.";
        Log.Error(error);
    }

    static public void Call(string name)
    {
        Func func;
        table_.TryGetValue(name, out func);
        if (func != null && func.func0 != null)
        {
            func.func0();
        }
        else
        {
            OutputFormatError(func);
        }
    }

    static public void Call<Arg0>(string name, Arg0 arg0)
    {
        Func func;
        table_.TryGetValue(name, out func);
        if (func != null && func.func1 != null)
        {
            func.func1(arg0);
        }
        else
        {
            OutputFormatError(func);
        }
    }

    static public void Call<Arg0, Arg1>(string name, Arg0 arg0, Arg1 arg1)
    {
        Func func;
        table_.TryGetValue(name, out func);
        if (func != null && func.func2 != null)
        {
            func.func2(arg0, arg1);
        }
        else
        {
            OutputFormatError(func);
        }
    }

    static public void Call<Arg0, Arg1, Arg2>(string name, Arg0 arg0, Arg1 arg1, Arg2 arg2)
    {
        Func func;
        table_.TryGetValue(name, out func);
        if (func != null && func.func3 != null)
        {
            func.func3(arg0, arg1, arg2);
        }
        else
        {
            OutputFormatError(func);
        }
    }

    [Command(name = "runtime-commands", description = "show run-time commands")]
    static public void ShowCommands()
    {
        var commands = table
            .Select(pair => string.Format(
                "- <b><i><color=#88ff88ff>{0}</color></i></b>\n" +
                "{1}",
                pair.Key,
                pair.Value.description))
            .Aggregate((str, x) => str + "\n" + x);
        Log.Output(commands);
    }

    static public bool ConvertIntoCodeIfCommand(ref string code)
    {
        var tempCode = code;

        // Remove last semicolon.
        tempCode = tempCode.TrimEnd(';');

        var commandLength = tempCode.IndexOf(" ");
        if (commandLength == -1) commandLength = tempCode.Length;
        var command = tempCode.Substring(0, commandLength);

        if (!HasRegistered(command)) return false;

        // Remove command and get only arguments.
        var argsStr = tempCode.Substring(commandLength);

        // Store parentheses.
        var parentheses = CommandUtil.ConvertParenToPlaceholder(tempCode);
        tempCode = parentheses.output;

        // Store quatation blocks.
        var quates = CommandUtil.ConvertQuateToPlaceholder(tempCode);
        tempCode = quates.output;

        // Split arguments with space.
        var args = argsStr.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);

        // Convert the command into the Class.Method() style.
        tempCode = string.Format("{0}(\"{1}\"{2}{3});",
            typeof(RuntimeCommands).FullName + ".Call",
            command,
            args.Length > 0 ? ", " : "",
            string.Join(", ", args));

        // Replace temporary quates placeholders to actual expressions.
        tempCode = CommandUtil.ConvertPlaceholderToBlock(tempCode, quates);

        // Replace temporary parentheses placeholders to actual expressions.
        tempCode = CommandUtil.ConvertPlaceholderToBlock(tempCode, parentheses);

        code = tempCode;

        return true;
    }
}

}