using System.Text;

namespace BindingTransform.Serialization.CSharp;

public class CSharpConstantSerializer
{
	public static string Serialize(ConstantDeclaration c, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		var constantType = c.Value.Contains("\"") ? "string"
			: c.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ? "bool"
			: c.Value.Equals("false", StringComparison.OrdinalIgnoreCase) ? "bool"
			: int.TryParse(c.Value, out _) ? "int"
			: uint.TryParse(c.Value, out _) ? "uint"
			: long.TryParse(c.Value, out _) ? "long"
			: ulong.TryParse(c.Value, out _) ? "ulong"
			: double.TryParse(c.Value, out _) ? "double"
			: throw new Exception($"Unknown constant type ({c}))");

		output.Append($"public const {constantType} {c.Name} = {c.Value};");
		return output.ToString();
	}
}
