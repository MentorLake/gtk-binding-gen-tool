using System.Text;
using System.Text.RegularExpressions;

namespace BindingTransform.Serialization.CSharp;

public static class CSharpMethodSerializer
{
	private const string CustomStringMarshallerAttribute = "[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NoNativeFreeStringMarshaller))]";

	public static string ToCSharpDecl(this MethodDeclaration m, List<LibraryDeclaration> libraries)
	{
		return $"{ToCSharpReturnType(m)} {m.Name}({string.Join(", ", m.Parameters.Select(a => a.ToCSharpString(libraries)))})";
	}

	public static string ToCSharpReturnType(this MethodDeclaration m)
	{
		var returnType = m.ReturnType.ToCString();
		returnType = returnType.ConvertToBuiltInTypes();
		if (returnType.Contains(".")) returnType = "G" + returnType.Split(".").Last();
		var isBuiltInType = StringExtensions.Keywords.Contains(returnType.Replace("[]", "").TrimEnd('*'));

		if (IsArrayReturnType(m) || (isBuiltInType && returnType.EndsWith("*"))) return "IntPtr";

		if (returnType.EndsWith("*")) returnType = returnType.ConvertToHandleType();
		if (returnType.EndsWith("*") && Regex.Match(returnType, @"^const .*$").Success) returnType = returnType.ToArrayType();
		if (Regex.Match(returnType, @"^const .*$").Success) returnType = returnType.Replace("const ", ""); // Log warning?
		return returnType;
	}

	private static bool IsArrayReturnType(this MethodDeclaration m)
	{
		return m.ReturnTypeComments.Contains("An array of");
	}

	public static string ToExternDefinition(this MethodDeclaration m, string libraryName, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		output.AppendLine($"\t[DllImport(Libraries.{libraryName})]");
		if (m.IsReturnDataOwnedByInstance && ToCSharpReturnType(m) == "string") output.AppendLine("\t" + CustomStringMarshallerAttribute);
		output.AppendLine($"\tinternal static extern {ToCSharpDecl(m, libraries)};");
		return output.ToString();
	}

	public static string ToExternConstructorDefinition(this MethodDeclaration m, string libraryName, string typeName, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var parameters = string.Join(", ", m.Parameters.Select(a => a.ToCSharpString(libraries)));
		output.AppendLine($"\t[DllImport(Libraries.{libraryName})]");
		if (m.IsReturnDataOwnedByInstance && ToCSharpReturnType(m) == "string") output.AppendLine("\t" + CustomStringMarshallerAttribute);
		output.AppendLine($"\tinternal static extern {typeName}Handle {m.Name}({parameters});");
		return output.ToString();
	}

	public static string ToConstructorAdaptor(this MethodDeclaration m, string className, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var parameters = string.Join(", ", m.Parameters.Select(a => a.ToCSharpString(libraries)));
		var methodName = m.Name.ToPascalCase().Replace(className, "");
		var returnType = ToCSharpReturnType(m);
		var externCall = $"{className}Externs.{m.Name}({string.Join(", ", m.Parameters.Select(p => p.ToCSharpArgument(libraries)))});";

		output.AppendLine($"\tpublic static {className}Handle {methodName}({parameters})");
		output.AppendLine("\t{");
		output.AppendLine($"\t\treturn {externCall}");
		output.AppendLine("\t}");
		return output.ToString();
	}

	public static string ToStaticClassMethodAdaptor(this MethodDeclaration m, string className, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var parameters = string.Join(", ", m.Parameters.Select(a => a.ToCSharpString(libraries)));
		var methodName = m.Name.ToPascalCase().Replace(className, "");
		var returnType = ToCSharpReturnType(m);
		var externCall = $"{className}Externs.{m.Name}({string.Join(", ", m.Parameters.Select(p => p.ToCSharpArgument(libraries)))});";

		output.AppendLine($"\tpublic static {returnType} {methodName}({parameters})");
		output.AppendLine("\t{");
		output.AppendLine($"\t\t{(returnType == "void" ? "" : "return ")}{externCall}");
		output.AppendLine("\t}");
		return output.ToString();
	}

	public static string ToInstanceMethodAdaptor(this MethodDeclaration m, string className, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var parameters = "this " + string.Join(", ", m.Parameters.Select(a => a.ToCSharpString(libraries)));
		parameters = Regex.Replace(parameters, @"this ([^ ]+)Handle(.*)", $"this {className}Handle$2");

		var methodName = m.Name.ToPascalCase().Replace(className, "");
		var returnType = ToCSharpReturnType(m);
		var externCall = $"{className}Externs.{m.Name}({string.Join(", ", m.Parameters.Select(p => p.ToCSharpArgument(libraries)))});";

		if (returnType == "void")
		{
			output.AppendLine($"\tpublic static {className}Handle {methodName}({parameters})");
			output.AppendLine("\t{");
			output.AppendLine($"\t\t{externCall}");
			output.AppendLine($"\t\treturn {m.Parameters.First().Name.NormalizeName()};");
			output.AppendLine("\t}");
		}
		else
		{
			output.AppendLine($"\tpublic static {returnType} {methodName}({parameters})");
			output.AppendLine("\t{");
			output.AppendLine($"\t\treturn {externCall}");
			output.AppendLine("\t}");
		}

		return output.ToString();
	}
}
