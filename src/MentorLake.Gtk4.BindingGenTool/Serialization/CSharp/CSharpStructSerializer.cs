using System.Text;

namespace BindingTransform.Serialization.CSharp;

public static class CSharpStructSerializer
{
	public static string Serialize(StructDeclaration s, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		output.AppendLine($"public class {s.Name}Handle : BaseSafeHandle");
		output.AppendLine("{");
		foreach (var constructor in s.Constructors) output.AppendLine(constructor.ToConstructorAdaptor(s.Name, libraries));
		output.AppendLine("}");
		output.AppendLine();

		output.AppendLine($"internal class {s.Name}Externs");
		output.AppendLine("{");
		foreach (var m in s.Constructors) output.AppendLine(m.ToExternConstructorDefinition(libraryDeclaration.Name, s.Name, libraries));
		output.AppendLine("}");
		output.AppendLine();

		output.Append($"public struct {s.Name}");
		output.AppendLine();
		output.AppendLine("{");
		foreach (var p in s.Properties) output.AppendLine($"\tpublic {p.GetCSharpTypeName()} {p.Name.NormalizeName()};");
		output.AppendLine("}");

		return output.ToString();
	}

	public static string GetCSharpTypeName(this StructProperty s)
	{
		if (s.Type == null) return "IntPtr";

		var paramType = s.Type.ToCString().ConvertToBuiltInTypes();
		var isBuiltInType = StringExtensions.Keywords.Contains(paramType.Replace("[]", "").TrimEnd('*'));

		if (paramType.EndsWith("*") && isBuiltInType)
		{
			paramType = paramType.ToArrayType();
		}

		if (paramType.EndsWith("*") && !paramType.StartsWith("const ") && !isBuiltInType)
		{
			paramType = paramType.ConvertToHandleType();
		}

		if (paramType.EndsWith("*") && (s.IsArray || paramType.StartsWith("const ")))
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
