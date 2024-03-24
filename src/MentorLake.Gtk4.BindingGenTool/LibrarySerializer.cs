using System.Text;
using BindingTransform;

public static class LibrarySerializer
{
	public static void WriteAllFiles(string outputBaseDirectory, List<LibraryConfig> allConfigs, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
	{
		var outputDir = Path.Join(outputBaseDirectory, libraryDeclaration.Name);
		if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
		Directory.CreateDirectory(outputDir);

		var ns = $"namespace MentorLake.Gtk4.{libraryDeclaration.Name};";
		var usings = allConfigs.Select(l => $"using MentorLake.Gtk4.{l.Namespace};").ToList();
		var header = $"using MentorLake.Gtk4.Graphene;\nusing MentorLake.Gtk4.Cairo;\nusing MentorLake.Gtk4.Harfbuzz;\nusing System.Runtime.InteropServices;\n{string.Join("\n", usings)}\n\n{ns}";

		foreach (var d in libraryDeclaration.Delegates)
		{
			var output = new StringBuilder();
			output.AppendLine(header);
			output.AppendLine();
			output.AppendLine("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
			output.AppendLine($"public delegate {d.ToCSharpDecl(libraries)};");
			output.AppendLine();
			File.WriteAllText(Path.Join(outputDir, d.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Aliases)
		{
			var output = new StringBuilder();
			output.AppendLine(header);
			output.AppendLine();
			output.Append($"public struct {s.Name}");
			output.AppendLine();
			output.AppendLine("{");
			if (s.Type.ToCString() != "void") output.AppendLine($"\tpublic {s.ToCSharpType()} Value;");
			output.AppendLine("}");
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Structs.Concat(libraryDeclaration.Unions))
		{
			var output = new StringBuilder();
			output.AppendLine(header);
			output.AppendLine();
			output.AppendLine($"public class {s.Name}Handle : BaseSafeHandle");
			output.AppendLine("{");
			output.AppendLine("}");
			output.AppendLine();
			output.Append($"public struct {s.Name}");
			output.AppendLine();
			output.AppendLine("{");
			foreach (var p in s.Properties) output.AppendLine($"\tpublic {p.GetCSharpTypeName()} {p.Name.NormalizeName()};");
			output.AppendLine("}");
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Flags)
		{
			var output = new StringBuilder();
			output.AppendLine(header);
			output.AppendLine();
			output.AppendLine("[Flags]");
			output.AppendLine($"public enum {s.Name}");
			output.AppendLine("{");

			for (var i = 0; i < s.Values.Count; i++)
			{
				var bitVal = i == 0 || i == 1 ? i : 1 << i - 1;
				output.Append("\t" + s.Values[i] + " = " + bitVal);
				if (i < s.Values.Count - 1) output.Append(",");
				output.AppendLine();
			}

			output.AppendLine("}");
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Enums.Concat(libraryDeclaration.Errors))
		{
			var output = new StringBuilder();
			output.AppendLine(header);
			output.AppendLine();
			output.AppendLine($"public enum {s.Name}");
			output.AppendLine("{");
			output.AppendLine(string.Join(",\n", s.Values.Select(v => "\t" + v)));
			output.AppendLine("}");
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Interfaces)
		{
			var output = new StringBuilder();
			output.AppendLine(header);
			output.AppendLine();
			output.AppendLine($"public class {s.Name}Handle {(!string.IsNullOrEmpty(s.ParentClassName) ? $" : {s.ParentClassName}Handle" : "" )}");
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
				output.AppendLine($"\t[DllImport(Libraries.{libraryDeclaration.Name})]");
				output.AppendLine($"\tinternal static extern {m.ToCSharpDecl(libraries)};");
			}

			output.AppendLine("}");
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		var allGlobalFunctions = libraryDeclaration.Functions.ToList();

		foreach (var c in libraryDeclaration.Classes)
		{
			var classFunctions = allGlobalFunctions
				.Where(f => f.Parameters.Any())
				.Where(f => f.Parameters.First().ToCSharpTypeWithModifiers(libraries) == c.Name + "Handle")
				.ToList();

			allGlobalFunctions = allGlobalFunctions.Except(classFunctions).ToList();
			var output = new StringBuilder();

			output.AppendLine(header);
			output.AppendLine();
			output.Append($"public class {c.Name}Handle");
			if (!string.IsNullOrEmpty(c.ParentClassName))
			{
				if (c.ParentClassName.Contains("."))
				{
					var parts = c.ParentClassName.Split(".");
					var libName = parts[0].Trim();
					var matchingLibraryConfig = allConfigs.First(l => l.Namespace == libName);
					var parentTypeName = matchingLibraryConfig.DeclPrefix + parts[1].Trim();
					output.Append(" : " + parentTypeName + "Handle");
				}
				else
				{
					output.Append(" : " + c.ParentClassName + "Handle");
				}
			}
			output.AppendLine();
			output.AppendLine("{");
			foreach (var m in c.Constructors) output.AppendLine(m.ToConstructorAdaptor(c.Name, libraries));
			foreach (var m in c.Functions) output.AppendLine(m.ToStaticClassMethodAdaptor(c.Name, libraries));
			output.AppendLine("}");
			output.AppendLine();

			if (c.Signals.Any())
			{
				output.AppendLine($"public class {c.Name}Signal");
				output.AppendLine("{");
				output.AppendLine("\tpublic string Value { get; set; }");
				output.AppendLine($"\tpublic {c.Name}Signal(string value) => Value = value;");
				output.AppendLine("}");
				output.AppendLine();
				output.AppendLine($"public static class {c.Name}Signals");
				output.AppendLine("{");
				foreach (var s in c.Signals) output.AppendLine($"\tpublic static {c.Name}Signal {s.ToPascalCase()} = new(\"{s}\");");
				output.AppendLine("}");
				output.AppendLine();
			}

			output.AppendLine($"public static class {c.Name}HandleExtensions");
			output.AppendLine("{");
			foreach (var m in c.Methods.Concat(classFunctions).DistinctBy(m => m.Name)) output.AppendLine(m.ToInstanceMethodAdaptor(c.Name, libraries));

			if (c.Signals.Any())
			{
				output.AppendLine($"\tpublic static {c.Name}Handle Connect(this {c.Name}Handle instance, {c.Name}Signal signal, GCallback c_handler)");
				output.AppendLine("\t{");
				output.AppendLine("\t\tGObjectExterns.g_signal_connect_data(instance, signal.Value, c_handler, IntPtr.Zero, null, GConnectFlags.G_CONNECT_AFTER);");
				output.AppendLine("\t\treturn instance;");
				output.AppendLine("\t}");
			}

			output.AppendLine("}");
			output.AppendLine();
			output.AppendLine($"internal class {c.Name}Externs");
			output.AppendLine("{");

			foreach (var m in c.Constructors)
			{
				var parameters = string.Join(", ", m.Parameters.Select(a => a.ToCSharpString(libraries)));
				output.AppendLine($"\t[DllImport(Libraries.{libraryDeclaration.Name})]");
				output.AppendLine($"\tinternal static extern {c.Name}Handle {m.Name}({parameters});");
			}

			foreach (var m in c.Methods.Concat(classFunctions).Concat(c.Functions).DistinctBy(m => m.Name))
			{
				output.AppendLine($"\t[DllImport(Libraries.{libraryDeclaration.Name})]");
				output.AppendLine($"\tinternal static extern {m.ToCSharpDecl(libraries)};");
			}

			output.AppendLine("}");
			File.WriteAllText(Path.Join(outputDir, $"{c.Name}.cs"), output.ToString());
		}

		var globalFunctionsOutput = new StringBuilder();
		globalFunctionsOutput.AppendLine(header);
		globalFunctionsOutput.AppendLine();
		globalFunctionsOutput.AppendLine("internal class GlobalFunctionExterns");
		globalFunctionsOutput.AppendLine("{");

		foreach (var f in allGlobalFunctions)
		{
			globalFunctionsOutput.AppendLine($"\t[DllImport(Libraries.{libraryDeclaration.Name})]");
			globalFunctionsOutput.AppendLine($"\tinternal static extern {f.ToCSharpDecl(libraries)};");
		}

		globalFunctionsOutput.AppendLine("}");
		File.WriteAllText(Path.Join(outputDir, "GlobalFunctionExterns.cs"), globalFunctionsOutput.ToString());
	}
}
