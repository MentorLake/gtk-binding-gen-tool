using System.Text.RegularExpressions;

namespace BindingTransform;

public static class CSharpParameterSerializer
{
	public static string ToCSharpString(this MethodParameter m, List<LibraryDeclaration> libraries)
	{
		return ToCSharpTypeWithModifiers(m, libraries) + " " + m.ToCSharpParameterName();
	}

	public static string ToCSharpTypeWithModifiers(this MethodParameter m, List<LibraryDeclaration> libraries)
	{
		if (m.IsVarArgs) return "IntPtr";
		var paramType = m.Type.ToCString().ConvertToBuiltInTypes();
		var isBuiltInType = StringExtensions.Keywords.Contains(paramType.Replace("[]", "").TrimEnd('*').Replace("const ", ""));

		if (paramType.Contains("."))
		{
			var library = libraries.First(l => l.Name == paramType.Split(".").First());
			paramType =  library.Config.DeclPrefix + paramType.Split(".").Last();
		}

		if (m.IsOutParameter()) paramType = "out " + Regex.Replace(paramType, @"^(.+)\*$", "$1");
		if (m.IsArray()) paramType = paramType.ToArrayType();

		if (isBuiltInType)
		{
			if (!m.IsOutParameter() && paramType.EndsWith("*")) paramType = "ref " + paramType.Substring(0, paramType.Length - 1);
			if (paramType.EndsWith("*")) paramType = paramType.ToArrayType();
			return paramType;
		}

		var isEnum = m.IsEnum(paramType.Replace("[]", "").TrimEnd('*'), libraries);
		var isAlias = m.IsAlias(paramType.Replace("[]", "").Replace("const ", "").TrimEnd('*'), libraries);
		if (isAlias && paramType.StartsWith("const ") && paramType.EndsWith("*")) paramType = paramType.ToArrayType();
		if (!isEnum && paramType.EndsWith("*")) paramType = paramType.ConvertToHandleType();
		if (!m.IsOutParameter() && paramType.EndsWith("*")) paramType = "ref " + paramType.Substring(0, paramType.Length - 1);
		if (paramType.EndsWith("*") && Regex.Match(paramType, @"^(out )?(ref )?const .*$").Success) paramType = paramType.ToArrayType();
		if (Regex.Match(paramType, @"^(out )?(ref )?const .*$").Success) paramType = paramType.Replace("const ", "");

		return paramType;
	}

	private static bool IsAlias(this MethodParameter m, string type, List<LibraryDeclaration> libraries)
	{
		return libraries.SelectMany(l => l.Aliases).Any(e => e.Name == type);
	}

	private static bool IsEnum(this MethodParameter m, string type, List<LibraryDeclaration> libraries)
	{
		return libraries.SelectMany(l => l.Enums.Concat(l.Flags).Concat(l.Errors)).Any(e => e.Name == type);
	}

	private static bool IsArray(this MethodParameter m)
	{
		return m.Comments.Contains("An array of");
	}

	private static bool IsOutParameter(this MethodParameter m)
	{
		if (m.Type.ToCString() == "GError**") return true;

		return m.Comments.Contains("The argument will be set by the function", StringComparison.OrdinalIgnoreCase)
		       || m.Comments.Contains("the caller will take ownership of the data", StringComparison.OrdinalIgnoreCase)
		       || m.Comments.Contains("return location for", StringComparison.OrdinalIgnoreCase);
	}

	public static string ToCSharpParameterName(this MethodParameter m)
	{
		if (m.IsVarArgs) return "@__arglist";
		return m.Name.NormalizeName();
	}

	public static string ToCSharpArgument(this MethodParameter m, List<LibraryDeclaration> libraries)
	{
		if (m.IsVarArgs) return m.ToCSharpParameterName();
		var paramType = m.ToCSharpTypeWithModifiers(libraries);
		if (paramType.StartsWith("out ")) return "out " + m.ToCSharpParameterName();
		if (paramType.StartsWith("ref ")) return "ref " + m.ToCSharpParameterName();
		return m.ToCSharpParameterName();
	}
}
