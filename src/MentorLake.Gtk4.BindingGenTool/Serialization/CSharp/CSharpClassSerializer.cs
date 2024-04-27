using System.Text;

namespace BindingTransform.Serialization.CSharp;

public class CSharpClassSerializer
{
	public static string Serialize(ClassDeclaration c, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		output.AppendLine($"public class {c.Name}Handle{SerializeInherited(c, libraries)}");
		output.AppendLine("{");
		foreach (var m in c.Constructors) output.AppendLine(m.ToConstructorAdaptor(c.Name, libraries));
		foreach (var m in c.Functions) output.AppendLine(m.ToStaticClassMethodAdaptor(c.Name, libraries));
		output.AppendLine("}");

		if (c.Signals.Any())
		{
			output.AppendLine().AppendLine(CSharpSignalsSerializer.Serialize(c, libraryDeclaration, libraries));
		}

		output.AppendLine();
		output.AppendLine($"public static class {c.Name}HandleExtensions");
		output.AppendLine("{");
		foreach (var m in c.Methods.DistinctBy(m => m.Name)) output.AppendLine(m.ToInstanceMethodAdaptor(c.Name, libraries));
		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"internal class {c.Name}Externs");
		output.AppendLine("{");
		foreach (var m in c.Constructors) output.AppendLine(m.ToExternConstructorDefinition(libraryDeclaration.Name, c.Name, libraries));
		foreach (var m in c.Methods.Concat(c.Functions).DistinctBy(m => m.Name)) output.AppendLine(m.ToExternDefinition(libraryDeclaration.Name, libraries));
		output.AppendLine("}");
		return output.ToString();
	}

	private static string SerializeInherited(ClassDeclaration c, List<LibraryDeclaration> libraries)
	{
		var inherited = new List<string>();
		inherited.Add(SerializeParentClassName(c, libraries));
		inherited.AddRange(SerializeInterfaces(c, libraries));
		inherited = inherited.Where(i => !string.IsNullOrEmpty(i)).ToList();
		if (inherited.Any()) return " : " + string.Join(", ", inherited);
		return "";
	}

	private static List<string> SerializeInterfaces(ClassDeclaration c, List<LibraryDeclaration> libraries)
	{
		return c.Interfaces
			.Select(i =>
			{
				if (i.Contains("."))
				{
					var parts = i.Split(".");
					var libName = parts[0].Trim();
					var matchingLibraryConfig = libraries.Select(l => l.Config).FirstOrDefault(l => l.SourceNamespace == libName);
					return matchingLibraryConfig.DeclPrefix + parts[1].Trim();
				}

				return i;
			})
			.Select(i => i + "Handle")
			.ToList();
	}

	private static string SerializeParentClassName(ClassDeclaration c, List<LibraryDeclaration> libraries)
	{
		if (!string.IsNullOrEmpty(c.ParentClassName))
		{
			if (c.ParentClassName.Contains("."))
			{
				var parts = c.ParentClassName.Split(".");
				var libName = parts[0].Trim();
				var matchingLibraryConfig = libraries.Select(l => l.Config).First(l => l.Namespace == libName);
				var parentTypeName = matchingLibraryConfig.DeclPrefix + parts[1].Trim();
				return parentTypeName + "Handle";
			}

			return c.ParentClassName + "Handle";
		}

		return "";
	}
}
