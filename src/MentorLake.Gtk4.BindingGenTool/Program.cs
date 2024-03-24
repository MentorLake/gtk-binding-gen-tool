namespace BindingTransform;

public static class Program
{
	private static List<LibraryConfig> Configs = new()
	{
		new() { Namespace = "GLib", DirectoryName = "glib", DeclPrefix = "G" },
		new() { Namespace = "GObject", DirectoryName = "gobject", DeclPrefix = "G", TypesToSkip = { "IOCondition" }},
		new() { Namespace = "Gio", DirectoryName = "gio", DeclPrefix = "G" },
		new() { Namespace = "GModule", DirectoryName = "gmodule", DeclPrefix = "G" },
		new() { Namespace = "Pango", DirectoryName = "Pango", DeclPrefix = "Pango" },
		new() { Namespace = "GdkPixbuf", DirectoryName = "gdk-pixbuf", DeclPrefix = "Gdk" },
		new() { Namespace = "Gdk4", DirectoryName = "gdk4", DeclPrefix = "Gdk" },
		new() { Namespace = "Gsk4", DirectoryName = "gsk4", DeclPrefix = "Gsk" },
		new() { Namespace = "Gtk4", DirectoryName = "gtk4", DeclPrefix = "Gtk" }
	};

	private static readonly string HomeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	private static readonly string LibraryDocsDirectory = Path.Join(HomeFolder, "Downloads/public");
	private static readonly string OutputBaseDirectory = Path.Join(HomeFolder, "Projects/MentorLake.Gtk4/src/MentorLake.Gtk4");

	public static async Task Main()
	{
		var parsedLibraries = new List<LibraryDeclaration>();

		foreach (var libraryConfig in Configs)
		{
			Console.WriteLine(libraryConfig.Namespace);

			var libraryDeclaration = new LibraryDeclaration();
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

		Console.WriteLine("Writing...");

		foreach (var lib in parsedLibraries)
		{
			LibrarySerializer.WriteAllFiles(OutputBaseDirectory, Configs, lib, parsedLibraries);
		}

		Console.WriteLine("Done");
	}
}
