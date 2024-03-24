using BindingTransform;

namespace MentorLake.Gtk4.BindingGenTool.Tests;

public class AdaptorTests
{
	[Test]
	public void g_object_replace_data()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			gboolean
			g_object_replace_data (
			  GObject* object,
			  const gchar* key,
			  gpointer oldval,
			  gpointer newval,
			  GDestroyNotify destroy,
			  GDestroyNotify* old_destroy
			)"));

		var methodDecl = parser.ReadMethodDeclaration();
		methodDecl.Parameters.Last().Comments = "The argument will be set by the function";

		var adaptorMethod = methodDecl.ToAdaptorMethod("GObject", new List<LibraryDeclaration>());
		var expectedOutput = @"	public static bool g_object_replace_data(this GObjectHandle @object, string key, IntPtr oldval, IntPtr newval, GDestroyNotify destroy, out GDestroyNotify old_destroy)
	{
		return GObjectExterns.g_object_replace_data(@object, key, oldval, newval, destroy, out old_destroy);
	}
";

		Assert.AreEqual(expectedOutput, adaptorMethod);
	}
}
