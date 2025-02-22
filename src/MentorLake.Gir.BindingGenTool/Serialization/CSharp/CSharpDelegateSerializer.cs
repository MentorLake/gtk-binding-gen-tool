using System.Text;

namespace BindingTransform.Serialization.CSharp;

public class CSharpDelegateSerializer
{
	public static string Serialize(MethodDeclaration d, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		output.AppendLine("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
		var parameters = string.Join(", ", d.Parameters.Select(a => CreateParameter(a, libraries)));
		output.AppendLine($"public delegate {d.ToCSharpReturnType()} {d.Name.NormalizeName()}({parameters});");
		return output.ToString();
	}

	private static string CreateParameter(MethodParameter param, List<LibraryDeclaration> libraries)
	{
		var attr = "";
		var typeWithModifiers = param.ToCSharpTypeWithModifiers(libraries);
		var paramName = param.ToCSharpParameterName();
		var typeName = typeWithModifiers.Split(" ").Last();

		if (typeWithModifiers.EndsWith("Handle"))
		{
			var isInterface = libraries.Any(l => l.Interfaces.Any(i => i.Name + "Handle" == typeName));
			var safeHandleType = typeName;
			if (isInterface) safeHandleType += "Impl";
			attr = $"[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DelegateSafeHandleMarshaller<{safeHandleType}>))] ";
		}
		return $"{attr}{typeWithModifiers} {paramName}";
	}
}
