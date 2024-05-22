using System.Text;
using System.Text.RegularExpressions;

namespace BindingTransform;

public static class StringExtensions
{
	public static string ToArrayType(this string name)
	{
		name = name.Substring(name.StartsWith("const ") ? 6 : 0);
		return Regex.Replace(name, @"^([^\*]*)\*(\**)$", "$1[]$2");
	}

	public static string ConvertToHandleType(this string name)
	{
		if (name.Contains("[]"))
		{
			return Regex.Replace(name, @"^([^\*]*)\[\]\*(.*)$", "$1Handle[]$2");
		}

		return Regex.Replace(name, @"^([^\*]*)\*(.*)$", "$1Handle$2");
	}

	private static bool TryConvert(string @in, string regex, string repl, out string @out)
	{
		@out = "";

		if (Regex.Match(@in, regex).Success)
		{
			@out = Regex.Replace(@in, regex, repl);
			return true;
		}

		return false;
	}

	public static string ConvertToBuiltInTypes(this string name)
	{
		if (TryConvert(name, @"^(const )?g?(u|uni)?char\*", "string", out var x)) return x;
		if (TryConvert(name, @"^(const )?g?(?:u|uni)?char\d*\*(\**)", "string$2", out x)) return x;
		if (TryConvert(name, @"^(const )?g?(u|uni)?char", "char", out x)) return x;
		if (Regex.Match(name, @"^(const )?g?u?char\* const\*").Success) return "string[]";
		if (Regex.Match(name, @"^(const )?void\*").Success) return "IntPtr";
		if (TryConvert(name, @"^?g?(u)?intptr", "n$1int", out x)) return x;
		if (TryConvert(name, @"^grefcount", "int", out x)) return x;
		if (TryConvert(name, @"^goffset", "int", out x)) return x;
		if (TryConvert(name, @"^(gpointer|gconstpointer)", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^gss?ize", "UIntPtr", out x)) return x;
		if (TryConvert(name, @"^size_t", "UIntPtr", out x)) return x;
		if (TryConvert(name, @"^time_t", "long", out x)) return x;
		if (TryConvert(name, @"^pid_t", "int", out x)) return x;
		if (TryConvert(name, @"^uid_t", "int", out x)) return x;
		if (TryConvert(name, @"^tm\*", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^FILE\*", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^Screen\*", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^Display\*", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^Window", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^Cursor", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^XID", "ulong", out x)) return x;
		if (TryConvert(name, @"^Atom", "ulong", out x)) return x;
		if (TryConvert(name, @"^passwd\*", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^utimbuf\*", "IntPtr", out x)) return x;
		if (TryConvert(name, @"^gatomicrefcount", "int", out x)) return x;
		if (TryConvert(name, @"^unsigned$", "int", out x)) return x;

		if (TryConvert(name, @"^(?:const )?g?(u)?int64(\[\])?", "$1long$2", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?(u)?int32(\[\])?", "$1int$2", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?(u)?int16(\[\])?", "$1short$2", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?u?int8(\[\])?", "byte$1", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?(u)?int(\[\])?", "$1int$2", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?double(\[\])?", "double$1", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?(u)?long(\[\])?", "$1long$2", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?(u)?short(\[\])?", "$1short$2", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?float(\[\])?", "float$1", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?char(\[\])?", "char$1", out x)) return x;
		if (TryConvert(name, @"^(?:const )?g?boolean(\[\])?", "bool$1", out x)) return x;
		if (Regex.Match(name, @"^va_list").Success) return "IntPtr";
		if (Regex.Match(name, @"^GObjectTypeInstance").Success) return "GTypeInstanceHandle";
		if (Regex.Match(name, @"^GObjectTypeClass").Success) return "GTypeClassHandle";
		if (Regex.Match(name, @"^GObjectTypeInterface").Success) return "GTypeInterfaceHandle";
		if (Regex.Match(name, @"^GCallback").Success) return "IntPtr";

		return name;
	}
	public static string NormalizeName(this string s)
	{
		return Keywords.Contains(s) ? "@" + s : s;
	}

	public static readonly string[] Keywords = { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
		"checked", "class", "const", "continue", "decimal", "default", "delegate",
		"do", "double", "else", "enum", "event", "explicit", "extern", "false",
		"finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
		"in", "int", "interface", "internal", "is", "lock", "long", "namespace",
		"new", "null", "object", "operator", "out", "override", "params", "private",
		"protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
		"short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
		"this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
		"unsafe", "ushort", "using", "using static", "virtual", "void", "volatile",
		"while", "yield", "IntPtr", "UIntPtr" };

	public static string ToPascalCase(this string s) {
		var result = new StringBuilder();
		var nonWordChars = new Regex(@"[^a-zA-Z0-9]+");
		var tokens = nonWordChars.Split(s);
		foreach (var token in tokens) {
			result.Append(PascalCaseSingleWord(token));
		}

		return result.ToString();
	}

	static string PascalCaseSingleWord(string s) {
		var match = Regex.Match(s, @"^(?<word>\d+|^[a-z]+|[A-Z]+|[A-Z][a-z]+|\d[a-z]+)+$");
		var groups = match.Groups["word"];

		var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
		var result = new StringBuilder();
		foreach (var capture in groups.Captures.Cast<Capture>()) {
			result.Append(textInfo.ToTitleCase(capture.Value.ToLower()));
		}
		return result.ToString();
	}
}
