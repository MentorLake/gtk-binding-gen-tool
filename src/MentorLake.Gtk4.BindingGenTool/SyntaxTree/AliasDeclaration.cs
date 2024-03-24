namespace BindingTransform;

public class AliasDeclaration
{
	public string Name { get; set; }
	public ParsedType Type { get; set; }

	public string ToCSharpType()
	{
		var paramType = Type.ToCString().ConvertToBuiltInTypes();
		var builtInTypeName = paramType.Replace("[]", "").TrimEnd('*');
		var isBuiltInType = StringExtensions.Keywords.Contains(builtInTypeName);

		if (paramType.EndsWith("*") && isBuiltInType)
		{
			paramType = paramType.Substring(0, paramType.Length - 1) + "[]";
		}

		return paramType;
	}
}
