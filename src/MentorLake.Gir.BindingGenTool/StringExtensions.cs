using System.Text;
using System.Text.RegularExpressions;

namespace BindingTransform;

public static class StringExtensions
{
	public static string NormalizeName(this string s)
	{
		var result = Keywords.Contains(s) ? "@" + s : s;
		return result.Replace('-', '_');
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
