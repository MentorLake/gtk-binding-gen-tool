using System.Text;
using System.Text.RegularExpressions;

namespace BindingTransform;

public class MethodDeclaration
{
	public ParsedType ReturnType { get; set; }
	public string ReturnTypeComments { get; set; } = "";
	public string Name { get; set; }
	public List<MethodParameter> Parameters { get; set; } = new();

	public string ToCSharpDecl(List<LibraryDeclaration> libraries)
	{
		return $"{ToCSharpReturnType()} {Name}({string.Join(", ", Parameters.Select(a => a.ToCSharpString(libraries)))})";
	}

	public string ToCSharpReturnType()
	{
		var returnType = ReturnType.ToCString();
		returnType = returnType.ConvertToBuiltInTypes();
		if (returnType.Contains(".")) returnType = "G" + returnType.Split(".").Last();
		var isBuiltInType = StringExtensions.Keywords.Contains(returnType.Replace("[]", "").TrimEnd('*'));

		if (IsArrayReturnType()) returnType = returnType.ToArrayType();

		if (isBuiltInType)
		{
			if (returnType.EndsWith("*")) returnType = returnType.ToArrayType();
			return returnType;
		}

		if (returnType.EndsWith("*")) returnType = returnType.ConvertToHandleType();
		if (returnType.EndsWith("*") && Regex.Match(returnType, @"^const .*$").Success) returnType = returnType.ToArrayType();
		if (Regex.Match(returnType, @"^const .*$").Success) returnType = returnType.Replace("const ", ""); // Log warning?
		return returnType;
	}

	private bool IsArrayReturnType()
	{
		return ReturnTypeComments.Contains("An array of");
	}

	public string ToConstructorAdaptor(string className, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var parameters = string.Join(", ", Parameters.Select(a => a.ToCSharpString(libraries)));
		var methodName = Name.ToPascalCase().Replace(className, "");
		var returnType = ToCSharpReturnType();
		var externCall = $"{className}Externs.{Name}({string.Join(", ", Parameters.Select(p => p.ToCSharpArgument(libraries)))});";

		output.AppendLine($"\tpublic static {className}Handle {methodName}({parameters})");
		output.AppendLine("\t{");
		output.AppendLine($"\t\treturn {externCall}");
		output.AppendLine("\t}");
		return output.ToString();
	}

	public string ToStaticClassMethodAdaptor(string className, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var parameters = string.Join(", ", Parameters.Select(a => a.ToCSharpString(libraries)));
		var methodName = Name.ToPascalCase().Replace(className, "");
		var returnType = ToCSharpReturnType();
		var externCall = $"{className}Externs.{Name}({string.Join(", ", Parameters.Select(p => p.ToCSharpArgument(libraries)))});";

		output.AppendLine($"\tpublic static {returnType} {methodName}({parameters})");
		output.AppendLine("\t{");
		output.AppendLine($"\t\t{(returnType == "void" ? "" : "return ")}{externCall}");
		output.AppendLine("\t}");
		return output.ToString();
	}

	public string ToInstanceMethodAdaptor(string className, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var parameters = "this " + string.Join(", ", Parameters.Select(a => a.ToCSharpString(libraries)));
		parameters = Regex.Replace(parameters, @"this ([^ ]+)Handle(.*)", $"this {className}Handle$2");

		var methodName = Name.ToPascalCase().Replace(className, "");
		var returnType = ToCSharpReturnType();
		var externCall = $"{className}Externs.{Name}({string.Join(", ", Parameters.Select(p => p.ToCSharpArgument(libraries)))});";

		if (returnType == "void")
		{
			output.AppendLine($"\tpublic static {className}Handle {methodName}({parameters})");
			output.AppendLine("\t{");
			output.AppendLine($"\t\t{externCall}");
			output.AppendLine($"\t\treturn {Parameters.First().Name.NormalizeName()};");
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
