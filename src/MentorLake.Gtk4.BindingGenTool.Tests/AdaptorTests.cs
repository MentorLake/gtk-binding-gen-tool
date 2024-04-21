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

		var adaptorMethod = methodDecl.ToInstanceMethodAdaptor("GObject", new List<LibraryDeclaration>());
		var expectedOutput = @"	public static bool ReplaceData(this GObjectHandle @object, string key, IntPtr oldval, IntPtr newval, GDestroyNotify destroy, out GDestroyNotify old_destroy)
	{
		return GObjectExterns.g_object_replace_data(@object, key, oldval, newval, destroy, out old_destroy);
	}
";

		Assert.AreEqual(expectedOutput, adaptorMethod);
	}

	[Test]
	public void g_param_spec_internal()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			GObject.ParamSpec*
			g_param_spec_internal (
			GType param_type,
			const gchar* name,
			const gchar* nick,
			const gchar* blurb,
			GParamFlags flags
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		var adaptorMethod = methodDecl.ToInstanceMethodAdaptor("GObject", new List<LibraryDeclaration>());
		var expectedOutput = @"	public static GParamSpecHandle GParamSpecInternal(this GType param_type, string name, string nick, string blurb, GParamFlags flags)
	{
		return GObjectExterns.g_param_spec_internal(param_type, name, nick, blurb, flags);
	}
";

		Console.WriteLine(adaptorMethod);
		Assert.AreEqual(expectedOutput, adaptorMethod);
	}
}
