using System.Text;

namespace BindingTransform.Serialization.CSharp;

public class CSharpDelegateSerializer
{
	public static string Serialize(MethodDeclaration d, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		output.AppendLine("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
		output.AppendLine($"public delegate {d.ToCSharpDecl(libraries)};");
		return output.ToString();
	}
}
