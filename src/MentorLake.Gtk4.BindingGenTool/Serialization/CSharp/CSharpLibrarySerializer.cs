using System.Text;
using BindingTransform;
using BindingTransform.Serialization.CSharp;

public static class LibrarySerializer
{
	public static void WriteAllFiles(string outputBaseDirectory, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
	{
		var outputDir = Path.Join(outputBaseDirectory, libraryDeclaration.Name);
		if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
		Directory.CreateDirectory(outputDir);

		var header = $"namespace MentorLake.Gtk4.{libraryDeclaration.Name};";

		foreach (var d in libraryDeclaration.Delegates)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(CSharpDelegateSerializer.Serialize(d, libraries));
			File.WriteAllText(Path.Join(outputDir, d.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Aliases)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(CSharpAliasSerializer.Serialize(s));
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Structs.Concat(libraryDeclaration.Unions))
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(CSharpStructSerializer.Serialize(s, libraryDeclaration, libraries));
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Flags)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(CSharpFlagSerializer.Serialize(s));
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Enums.Concat(libraryDeclaration.Errors))
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(CSharpEnumSerializer.Serialize(s));
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in libraryDeclaration.Interfaces)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(CSharpInterfaceSerializer.Serialize(s, libraryDeclaration, libraries));
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var c in libraryDeclaration.Classes)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(CSharpClassSerializer.Serialize(c, libraryDeclaration, libraries));
			File.WriteAllText(Path.Join(outputDir, $"{c.Name}.cs"), output.ToString());
		}

		var globalFunctionsOutput = new StringBuilder();
		globalFunctionsOutput.AppendLine(header);
		globalFunctionsOutput.AppendLine();
		globalFunctionsOutput.AppendLine($"internal class {libraryDeclaration.Name}GlobalFunctionExterns");
		globalFunctionsOutput.AppendLine("{");

		foreach (var f in libraryDeclaration.Functions)
		{
			globalFunctionsOutput.AppendLine(f.ToExternDefinition(libraryDeclaration.Name, libraries));
		}

		globalFunctionsOutput.AppendLine("}");
		File.WriteAllText(Path.Join(outputDir, $"{libraryDeclaration.Name}GlobalFunctionExterns.cs"), globalFunctionsOutput.ToString());
	}
}
