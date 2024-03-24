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

	public string ToAdaptorMethod(string className, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var useFluentDsl = ReturnType.ToCString() == "void" && Parameters.Any() && Parameters.First().ToCSharpTypeWithModifiers(libraries).EndsWith("Handle");

		if (useFluentDsl)
		{
			if (Parameters.First().ToCSharpTypeWithModifiers(libraries) == className + "Handle")
			{
				output.AppendLine($"\tpublic static {className}Handle {Name.ToPascalCase().Replace(className, "")}(this {string.Join(", ", Parameters.Select(a => a.ToCSharpString(libraries)))})");
				output.AppendLine("\t{");
				output.AppendLine($"\t\t{className}Externs.{Name}({string.Join(", ", Parameters.Select(p => p.ToCSharpArgument(libraries)))});");
				output.AppendLine($"\t\treturn {Parameters.First().Name.NormalizeName()};");
				output.AppendLine("\t}");
			}
			else
			{
				output.AppendLine($"\tpublic static {className}Handle {Name.ToPascalCase().Replace(className, "")}(this {className}Handle @handle, {string.Join(", ", Parameters.Select(a => a.ToCSharpString(libraries)))})");
				output.AppendLine("\t{");
				output.AppendLine($"\t\t{className}Externs.{Name}({string.Join(", ", Parameters.Select(p => p.ToCSharpArgument(libraries)))});");
				output.AppendLine($"\t\treturn @handle;");
				output.AppendLine("\t}");
			}
		}
		else
		{
			output.AppendLine($"\tpublic static {ToCSharpReturnType()} {Name.ToPascalCase().Replace(className, "")}(this {string.Join(", ", Parameters.Select(a => a.ToCSharpString(libraries)))})");
			output.AppendLine("\t{");
			output.AppendLine($"\t\treturn {className}Externs.{Name}({string.Join(", ", Parameters.Select(p => p.ToCSharpArgument(libraries)))});");
			output.AppendLine("\t}");
		}

		return output.ToString();
	}
}
