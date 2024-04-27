using System.Text;

namespace BindingTransform.Serialization.CSharp;

public static class CSharpAliasSerializer
{
	public static string Serialize(AliasDeclaration s)
	{
		var output = new StringBuilder();
		output.Append($"public struct {s.Name}");
		output.AppendLine();
		output.AppendLine("{");
		if (s.Type.ToCString() != "void") output.AppendLine($"\tpublic {s.ToCSharpType()} Value;");
		output.AppendLine("}");
		return output.ToString();
	}

	public static string ToCSharpType(this AliasDeclaration s)
	{
		var paramType = s.Type.ToCString().ConvertToBuiltInTypes();
		var builtInTypeName = paramType.Replace("[]", "").TrimEnd('*');
		var isBuiltInType = StringExtensions.Keywords.Contains(builtInTypeName);

		if (paramType.EndsWith("*") && isBuiltInType)
		{
			paramType = paramType.Substring(0, paramType.Length - 1) + "[]";
		}

		return paramType;
	}
}
