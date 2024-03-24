using System.Text.RegularExpressions;

namespace BindingTransform;

public class MethodParameter
{
	public ParsedType Type { get; set; }
	public string Name { get; set; }
	public bool IsVarArgs { get; set; }
	public string Comments { get; set; } = "";

	public string ToCSharpString(List<LibraryDeclaration> libraries)
	{
		return ToCSharpTypeWithModifiers(libraries) + " " + ToCSharpParameterName();
	}

	public string ToCSharpTypeWithModifiers(List<LibraryDeclaration> libraries)
	{
		if (IsVarArgs) return "IntPtr";
		var paramType = Type.ToCString().ConvertToBuiltInTypes();
		var isBuiltInType = StringExtensions.Keywords.Contains(paramType.Replace("[]", "").TrimEnd('*').Replace("const ", ""));

		if (IsOutParameter()) paramType = "out " + Regex.Replace(paramType, @"^(.+)\*$", "$1");
		if (IsArray()) paramType = paramType.ToArrayType();

		if (isBuiltInType)
		{
			if (!IsOutParameter() && paramType.EndsWith("*")) paramType = "ref " + paramType.Substring(0, paramType.Length - 1);
			if (paramType.EndsWith("*")) paramType = paramType.ToArrayType();
			return paramType;
		}

		var isEnum = IsEnum(paramType.Replace("[]", "").TrimEnd('*'), libraries);
		var isAlias = IsAlias(paramType.Replace("[]", "").Replace("const ", "").TrimEnd('*'), libraries);
		if (isAlias && paramType.StartsWith("const ") && paramType.EndsWith("*")) paramType = paramType.ToArrayType();
		if (!isEnum && paramType.EndsWith("*")) paramType = paramType.ConvertToHandleType();
		if (!IsOutParameter() && paramType.EndsWith("*")) paramType = "ref " + paramType.Substring(0, paramType.Length - 1);
		if (paramType.EndsWith("*") && Regex.Match(paramType, @"^(out )?(ref )?const .*$").Success) paramType = paramType.ToArrayType();
		if (Regex.Match(paramType, @"^(out )?(ref )?const .*$").Success) paramType = paramType.Replace("const ", "");

		return paramType;
	}

	private bool IsAlias(string type, List<LibraryDeclaration> libraries)
	{
		return libraries.SelectMany(l => l.Aliases).Any(e => e.Name == type);
	}

	private bool IsEnum(string type, List<LibraryDeclaration> libraries)
	{
		return libraries.SelectMany(l => l.Enums.Concat(l.Flags).Concat(l.Errors)).Any(e => e.Name == type);
	}

	private bool IsArray()
	{
		return Comments.Contains("An array of");
	}

	private bool IsOutParameter()
	{
		if (Type.ToCString() == "GError**") return true;

		return Comments.Contains("The argument will be set by the function", StringComparison.OrdinalIgnoreCase)
		       || Comments.Contains("the caller will take ownership of the data", StringComparison.OrdinalIgnoreCase)
		       || Comments.Contains("return location for", StringComparison.OrdinalIgnoreCase);
	}

	public string ToCSharpParameterName()
	{
		if (IsVarArgs) return "@__arglist";
		return Name.NormalizeName();
	}

	public string ToCSharpArgument(List<LibraryDeclaration> libraries)
	{
		if (IsVarArgs) return ToCSharpParameterName();
		var paramType = ToCSharpTypeWithModifiers(libraries);
		if (paramType.StartsWith("out ")) return "out " + ToCSharpParameterName();
		if (paramType.StartsWith("ref ")) return "ref " + ToCSharpParameterName();
		return ToCSharpParameterName();
	}
}
