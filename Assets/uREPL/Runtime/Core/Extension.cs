using System.Globalization;

namespace uREPL
{

public static class Extensions
{
    private static CultureInfo cultureInfo = new CultureInfo("en-US");

    public static string AsString(this float value)
    {
        return value.ToString(cultureInfo);
    }

    public static string AsString(this object value)
    {
        return 
            (value is float) ? ((float)value).AsString() :
            value.ToString();
    }

    public static float AsFloat(this string value)
    {
        return float.Parse(value, cultureInfo);
    }
}

}
