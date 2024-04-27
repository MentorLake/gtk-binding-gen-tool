using System.Text.Json;
using System.Xml.Linq;

namespace BindingTransform;

public static class Program
{
	private static List<LibraryConfig> Configs = new()
	{
		new() { Namespace = "GLib", DirectoryName = "glib", DeclPrefix = "G", SourceNamespace = "GLib" },
		new() { Namespace = "GObject", DirectoryName = "gobject", DeclPrefix = "G", TypesToSkip = { "IOCondition" }, SourceNamespace = "GObject"},
		new() { Namespace = "Gio", DirectoryName = "gio", DeclPrefix = "G", SourceNamespace = "Gio" },
		new() { Namespace = "GModule", DirectoryName = "gmodule", DeclPrefix = "G", SourceNamespace = "GModule" },
		new() { Namespace = "Pango", DirectoryName = "Pango", DeclPrefix = "Pango", SourceNamespace = "Pango" },
		new() { Namespace = "GdkPixbuf", DirectoryName = "gdk-pixbuf", DeclPrefix = "Gdk", SourceNamespace = "GdkPixbuf" },
		new() { Namespace = "Gdk4", DirectoryName = "gdk4", DeclPrefix = "Gdk", SourceNamespace = "Gdk" },
		new() { Namespace = "Gsk4", DirectoryName = "gsk4", DeclPrefix = "Gsk", SourceNamespace = "Gsk" },
		new() { Namespace = "Gtk4", DirectoryName = "gtk4", DeclPrefix = "Gtk", SourceNamespace = "Gtk" }
	};

	private static readonly string HomeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	private static readonly string LibraryDocsDirectory = Path.Join(HomeFolder, "Downloads/public");
	private static readonly string OutputBaseDirectory = Path.Join(HomeFolder, "Projects/MentorLake.Gtk4/src/MentorLake.Gtk4");
	private static string UserDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
	private static string DocsJsonFilePath = Path.Join(UserDirectory, "docs.json");

	public static Task Main()
	{
		//ParseDocs();
		Console.WriteLine("Loading doc JSON...");
		var parsedLibraries = (List<LibraryDeclaration>) JsonSerializer.Deserialize(File.ReadAllText(DocsJsonFilePath), typeof(List<LibraryDeclaration>));

		foreach (var lib in parsedLibraries)
		{
			Console.WriteLine($"Writing {lib.Name}...");
			LibrarySerializer.WriteAllFiles(OutputBaseDirectory, lib, parsedLibraries);
		}

		Console.WriteLine("Done");
		return Task.CompletedTask;
	}

	private static void ParseDocs()
	{
		var parsedLibraries = new List<LibraryDeclaration>();

		foreach (var libraryConfig in Configs)
		{
			Console.WriteLine(libraryConfig.Namespace);

			var libraryDeclaration = new LibraryDeclaration();
			libraryDeclaration.Config = libraryConfig;
			libraryDeclaration.Name = libraryConfig.Namespace;
			parsedLibraries.Add(libraryDeclaration);

			var directory = Path.Join(LibraryDocsDirectory, libraryConfig.DirectoryName);

			libraryDeclaration.Delegates = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("callback."))
				.Where(f => f.EndsWith(".html"))
				.Select(f => DocsParser.ParseCallbackHtml(f))
				.ToList();

			libraryDeclaration.Functions = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("func."))
				.Where(f => f.EndsWith(".html"))
				.Select(f => DocsParser.ParseMethodHtml(f))
				.Where(f => f != null)
				.ToList();

			libraryDeclaration.Errors = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("error."))
				.Where(f => f.EndsWith(".html"))
				.Select(f => DocsParser.ParseEnumHtml(libraryConfig, f))
				.ToList();

			libraryDeclaration.Flags = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("flags."))
				.Where(f => f.EndsWith(".html"))
				.Where(f => !libraryConfig.TypesToSkip.Contains(Path.GetFileNameWithoutExtension(f).Replace("flags.", "")))
				.Select(f => DocsParser.ParseEnumHtml(libraryConfig, f))
				.ToList();

			libraryDeclaration.Enums = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("enum."))
				.Where(f => f.EndsWith(".html"))
				.Where(f => !libraryConfig.TypesToSkip.Contains(Path.GetFileNameWithoutExtension(f).Replace("enum.", "")))
				.Select(f => DocsParser.ParseEnumHtml(libraryConfig, f))
				.ToList();

			libraryDeclaration.Classes = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("class."))
				.Where(f => f.EndsWith(".html"))
				.Select(f => DocsParser.ParseClassHtml(libraryConfig, f))
				.ToList();

			libraryDeclaration.Interfaces = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("iface."))
				.Where(f => f.EndsWith(".html"))
				.Select(f => DocsParser.ParseInterfaceHtml(libraryConfig, f))
				.ToList();

			libraryDeclaration.Unions = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("union."))
				.Where(f => f.EndsWith(".html"))
				.Select(f => DocsParser.ParseStruct(f))
				.ToList();

			libraryDeclaration.Structs = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("struct."))
				.Where(f => f.EndsWith(".html"))
				.Select(f => DocsParser.ParseStruct(f))
				.ToList();

			libraryDeclaration.Aliases = Directory.GetFiles(directory)
				.Where(f => Path.GetFileName(f).StartsWith("alias."))
				.Where(f => f.EndsWith(".html"))
				.Select(f => DocsParser.ParseAlias(f))
				.ToList();
		}

		foreach (var l in parsedLibraries)
		{
			var allGlobalFunctions = l.Functions.ToList();

			foreach (var c in l.Classes)
			{
				var classFunctions = allGlobalFunctions
					.Where(f => f.Parameters.Any())
					.Where(f => f.Parameters.First().ToCSharpTypeWithModifiers(parsedLibraries) == c.Name + "Handle")
					.ToList();

				c.Methods.AddRange(classFunctions);
				allGlobalFunctions = allGlobalFunctions.Except(classFunctions).ToList();
			}

			l.Functions = allGlobalFunctions;
		}

		File.WriteAllBytes(DocsJsonFilePath, JsonSerializer.SerializeToUtf8Bytes(parsedLibraries));
		Console.WriteLine("Parsing done");
	}
}
