using System.Xml.Serialization;
using BindingTransform.Serialization.Gir;
using MentorLake.Gir.Core;

namespace BindingTransform;

public static class Program
{
	private static readonly string s_homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	private static readonly string s_outputBaseDirectory = Path.Join(s_homeFolder, "Projects/MentorLake.Gtk3/src/MentorLake.Gtk3");
	private static readonly string s_userDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

	public static Task Main()
	{
		var parsedLibraries = Directory.GetFiles(Path.Join(s_userDirectory, "Projects/MentorLake.Gir.BindingGenTool/src/MentorLake.Gir.Types/GirFiles/GTK3"), "*.gir")
			.Select(f =>
			{
				var serializer = new XmlSerializer(typeof(Repository));
				return (Repository) serializer.Deserialize(new StringReader(File.ReadAllText(f)));
			})
			.ToList();

		var serializer = new GirLibrarySerializer(parsedLibraries);

		foreach (var lib in parsedLibraries)
		{
			Console.WriteLine($"Writing {lib.Namespace.First().Name}...");
			serializer.WriteAllFiles(s_outputBaseDirectory, lib);
		}

		Console.WriteLine("Done");
		return Task.CompletedTask;
	}
}
