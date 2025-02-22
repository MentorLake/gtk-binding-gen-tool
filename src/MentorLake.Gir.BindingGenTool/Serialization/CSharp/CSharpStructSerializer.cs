using System.Runtime.InteropServices;
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

		output.AppendLine();
		output.AppendLine($"public static class {s.Name}HandleExtensions");
		output.AppendLine("{");
		foreach (var m in s.Methods.DistinctBy(m => m.Name)) output.AppendLine(m.ToInstanceMethodAdaptor(s.Name, libraries));
		output.AppendLine("}");

		output.AppendLine($"internal class {s.Name}Externs");
		output.AppendLine("{");
		foreach (var m in s.Constructors) output.AppendLine(m.ToExternConstructorDefinition(libraryDeclaration.Name, s.Name, libraries));
		foreach (var m in s.Methods) output.AppendLine(m.ToExternDefinition(libraryDeclaration.Name, libraries));
		output.AppendLine("}");
		output.AppendLine();

		// output.AppendLine($"internal struct Internal_{s.Name}");
		// output.AppendLine("{");
		// foreach (var p in s.Properties) output.AppendLine($"\t{p.GetUntypedFieldDecl()}");
		// output.AppendLine();
		// output.AppendLine(GetToTypedMethod(s, libraryDeclaration, libraries));
		// output.AppendLine("}");
		output.Append($"public struct {s.Name}");
		output.AppendLine();
		output.AppendLine("{");
		foreach (var p in s.Properties) output.AppendLine($"\t{p.GetUntypedFieldDecl()}");
		output.AppendLine("}");

		return output.ToString();
	}

// 	private static string GetToTypedMethod(this StructDeclaration s, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
// 	{
// 		var copy = new StringBuilder();
// 		var safeHandleCounter = 0;
//
// 		foreach (var p in s.Properties)
// 		{
// 			if (p.Type == null)
// 			{
// 				copy.AppendLine($"\t\ttypedInstance.{p.Name.NormalizeName()} = {p.Name.NormalizeName()};");
// 			}
// 			else
// 			{
// 				var paramType = p.Type.ToCString().ConvertToBuiltInTypes();
// 				var isBuiltInType = StringExtensions.Keywords.Contains(paramType.Replace("[]", "").TrimEnd('*'));
//
// 				if (paramType.EndsWith("*") && isBuiltInType)
// 				{
// 					copy.AppendLine($"\t\ttypedInstance.{p.Name.NormalizeName()} = {p.Name.NormalizeName()};");
// 				}
// 				else if (paramType.EndsWith("*") && !paramType.StartsWith("const ") && !isBuiltInType)
// 				{
// 					copy.AppendLine($"\t\tvar safeHandle{safeHandleCounter} = new {paramType.ConvertToHandleType()}();");
// 					copy.AppendLine($"\t\tMarshal.InitHandle(safeHandle{safeHandleCounter}, {p.Name.NormalizeName()});");
// 					copy.AppendLine($"\t\ttypedInstance.{p.Name.NormalizeName()} = safeHandle{safeHandleCounter++};");
// 				}
// 				else if (paramType.EndsWith("*") && (p.IsArray || paramType.StartsWith("const ")))
// 				{
// 					copy.AppendLine($"\t\ttypedInstance.{p.Name.NormalizeName()} = {p.Name.NormalizeName()};");
// 				}
// 				else if (paramType.EndsWith("*"))
// 				{
// 					copy.AppendLine($"\t\ttypedInstance.{p.Name.NormalizeName()} = {p.Name.NormalizeName()};");
// 				}
// 				else
// 				{
// 					copy.AppendLine($"\t\ttypedInstance.{p.Name.NormalizeName()} = {p.Name.NormalizeName()};");
// 				}
// 			}
// 		}
//
// 		return $@"
// 	public {s.Name} ToTyped()
// 	{{
// 		var typedInstance = new {s.Name}();
// {copy}
// 		return typedInstance;
// 	}}";
// 	}
//
	public static string GetUntypedFieldDecl(this StructProperty p)
	{
		if (p.Type == null) return $"public IntPtr {p.Name.NormalizeName()};";

		var paramType = p.Type.ToCString().ConvertToBuiltInTypes();
		var isBuiltInType = StringExtensions.Keywords.Contains(paramType.Replace("[]", "").TrimEnd('*'));

		if (paramType.EndsWith("*") && isBuiltInType)
		{
			return $"public IntPtr {p.Name.NormalizeName()};";
		}

		if (paramType.EndsWith("*") && !paramType.StartsWith("const ") && !isBuiltInType)
		{
			return $"public IntPtr {p.Name.NormalizeName()};";
		}

		if (paramType.EndsWith("*") && (p.IsArray || paramType.StartsWith("const ")))
		{
			return $"public IntPtr {p.Name.NormalizeName()};";
		}

		if (paramType.EndsWith("*"))
		{
			return $"public IntPtr {p.Name.NormalizeName()};";
		}

		return $"public {paramType} {p.Name.NormalizeName()};";
	}
//
// 	public static string GetTypedFieldDecl(this StructProperty p)
// 	{
// 		var name = p.Name.NormalizeName();
// 		if (p.Type == null) return $"public IntPtr {name};";
//
// 		var paramType = p.Type.ToCString().ConvertToBuiltInTypes();
// 		var isBuiltInType = StringExtensions.Keywords.Contains(paramType.Replace("[]", "").TrimEnd('*'));
//
// 		if (paramType.EndsWith("*") && isBuiltInType)
// 		{
// 			return $"public {paramType.ToArrayType()} {name};";
// 		}
//
// 		if (paramType.EndsWith("*") && !paramType.StartsWith("const ") && !isBuiltInType)
// 		{
// 			return $"public {paramType.ConvertToHandleType()} {name};";
// 		}
//
// 		if (paramType.EndsWith("*") && (p.IsArray || paramType.StartsWith("const ")))
// 		{
// 			paramType = paramType.Substring(paramType.StartsWith("const ") ? 6 : 0).ToArrayType();
// 			return $"public {paramType} {name};";
// 		}
//
// 		if (paramType.EndsWith("*"))
// 		{
// 			return $"public IntPtr {name};";
// 		}
//
// 		return $"public {paramType} {name};";
// 	}
}
