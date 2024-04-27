using System.Text;

namespace BindingTransform.Serialization.CSharp;

public class CSharpSignalsSerializer
{
	public static string Serialize(ClassDeclaration c, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		output.AppendLine($"public static class {c.Name}SignalExtensions");
		output.AppendLine("{");
		foreach (var signal in c.Signals)
		{
			output.AppendLine($"\tpublic static {c.Name}Handle Signal_{signal.Name.ToPascalCase()}(this {c.Name}Handle instance, {c.Name}SignalDelegates.{signal.Name.ToPascalCase()} handler)");
			output.AppendLine("\t{");
			output.AppendLine($"\t\tGObjectExterns.g_signal_connect_data(instance, \"{signal.Name}\", Marshal.GetFunctionPointerForDelegate(handler), IntPtr.Zero, null, GConnectFlags.G_CONNECT_AFTER);");
			output.AppendLine("\t\treturn instance;");
			output.AppendLine("\t}");
		}
		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"public static class {c.Name}SignalDelegates");
		output.AppendLine("{");

		foreach (var s in c.Signals)
		{
			var parameters = string.Join(", ", s.Parameters.Select(a => a.ToCSharpString(libraries)));
			if (s.Parameters.Any())
			{
				var customMarshaller = $"[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DelegateSafeHandleMarshaller<{c.Name}Handle>))]";
				var firstParam = customMarshaller + " " + s.Parameters.First().ToCSharpString(libraries);
				parameters = string.Join(", ", new[] { firstParam }.Concat(s.Parameters.Skip(1).Select(a => a.ToCSharpString(libraries))));
			}

			var decl = $"{s.ToCSharpReturnType()} {s.Name.ToPascalCase()}({parameters})";
			output.AppendLine();
			output.AppendLine("\t[UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
			output.AppendLine($"\tpublic delegate {decl};");
		}

		output.AppendLine("}");
		return output.ToString();
	}
}
