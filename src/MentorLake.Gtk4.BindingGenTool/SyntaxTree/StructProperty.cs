using System.Text.RegularExpressions;

namespace BindingTransform;

public class StructProperty
{
	public string Name { get; set; }
	public ParsedType Type { get; set; }
	public MethodDeclaration Func { get; set; }
	public int Bits { get; set; }
	public string Comments { get; set; }

	private bool IsArray()
	{
		return Comments.Contains("A pointer to a NULL-terminated array");
	}

	public string GetCSharpTypeName()
	{
		if (Type == null) return "IntPtr";

		var paramType = Type.ToCString().ConvertToBuiltInTypes();
		var isBuiltInType = StringExtensions.Keywords.Contains(paramType.Replace("[]", "").TrimEnd('*'));

		if (paramType.EndsWith("*") && isBuiltInType)
		{
			paramType = paramType.ToArrayType();
		}

		if (paramType.EndsWith("*") && !paramType.StartsWith("const ") && !isBuiltInType)
		{
			paramType = paramType.ConvertToHandleType();
		}

		if (paramType.EndsWith("*") && (IsArray() || paramType.StartsWith("const ")))
		{
			paramType = paramType.Substring(paramType.StartsWith("const ") ? 6 : 0).ToArrayType();
		}

		if (paramType.EndsWith("*"))
		{
			paramType = "IntPtr";
		}

		return paramType;
	}
}
