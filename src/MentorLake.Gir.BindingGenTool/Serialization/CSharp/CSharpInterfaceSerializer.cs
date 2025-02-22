using System.Text;

namespace BindingTransform.Serialization.CSharp;

public class CSharpInterfaceSerializer
{
	public static string Serialize(InterfaceDeclaration s, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		output.AppendLine($"public interface {s.Name}Handle");
		output.AppendLine("{");
		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"internal class {s.Name}HandleImpl : BaseSafeHandle, {s.Name}Handle");
		output.AppendLine("{");
		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"public static class {s.Name}HandleExtensions");
		output.AppendLine("{");
		foreach (var m in s.Methods) output.AppendLine(m.ToInstanceMethodAdaptor(s.Name, libraries));
		output.AppendLine("}");
		output.AppendLine();
		output.AppendLine($"internal class {s.Name}Externs");
		output.AppendLine("{");

		foreach (var m in s.Methods)
		{
			output.AppendLine(m.ToExternDefinition(libraryDeclaration.Name, libraries));
		}

		output.AppendLine("}");
		return output.ToString();
	}
}
