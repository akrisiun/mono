using System.Globalization;

/* partial class SR {
  
    public static string Format(string f, params object[] parm)
        => string.Format(f, parm);  

    internal static string GetString(string name, params object[] args)
        => SR2.GetString(System.Globalization.CultureInfo.InvariantCulture, name, args);
*/

static partial class SR2
{
    public static string Format(string f, params string[] parm) 
        => string.Format(f, parm);

	internal static string GetString(string name, params object[] args)
	{
		return GetString (CultureInfo.InvariantCulture, name, args);
	}

	internal static string GetString(CultureInfo culture, string name, params object[] args)
	{
		return string.Format (culture, name, args);
	}

	internal static string GetString(string name)
	{
		return name;
	}

	internal static string GetString(CultureInfo culture, string name)
	{
		return name;
	}
}

namespace System.Runtime.CompilerServices
{
	//class FriendAccessAllowedAttribute : Attribute
	//{ }

    class FriendAccessAllowedAttribute2 : Attribute
	{
        // internal FriendAccessAllowedAttribute Friend { get; set; }
    }
}
