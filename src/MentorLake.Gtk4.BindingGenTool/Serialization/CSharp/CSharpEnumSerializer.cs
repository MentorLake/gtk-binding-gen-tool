using System.Text;

namespace BindingTransform.Serialization.CSharp;

public class CSharpEnumSerializer
{
	public static string Serialize(EnumDeclaration decl)
	{
		var output = new StringBuilder();
		output.AppendLine($"public enum {decl.Name}");
		output.AppendLine("{");
		output.AppendLine(string.Join(",\n", decl.Values.Select(v => "\t" + v)));
		output.AppendLine("}");
		return output.ToString();
	}
}
