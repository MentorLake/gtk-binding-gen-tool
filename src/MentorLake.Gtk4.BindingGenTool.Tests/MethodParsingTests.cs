using BindingTransform;

namespace MentorLake.Gtk4.BindingGenTool.Tests;

public class MethodParsingTests
{
	public List<LibraryDeclaration> _libraries = new List<LibraryDeclaration>();

	[Test]
	public void g_dataset_destroy()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			void
			g_dataset_destroy (
			  gconstpointer dataset_location
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_dataset_destroy"));
			Assert.That(methodDecl.Parameters.First().ToCSharpTypeWithModifiers(_libraries), Is.EqualTo("IntPtr"));
		});
	}

	[Test]
	public void g_usleep()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			void
			g_usleep (
			  gulong microseconds
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_usleep"));
			Assert.That(methodDecl.Parameters.First().ToCSharpTypeWithModifiers(_libraries), Is.EqualTo("ulong"));
		});
	}

	[Test]
	public void g_datalist_id_dup_data()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			gpointer
			g_datalist_id_dup_data (
			  GData** datalist,
			  GQuark key_id,
			  GDuplicateFunc dup_func,
			  IntPtr user_data
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_datalist_id_dup_data"));
			Assert.That(methodDecl.Parameters.First().ToCSharpString(_libraries), Is.EqualTo("ref GDataHandle datalist"));
			Assert.That(methodDecl.Parameters.ElementAt(1).ToCSharpString(_libraries), Is.EqualTo("GQuark key_id"));
			Assert.That(methodDecl.Parameters.ElementAt(2).ToCSharpString(_libraries), Is.EqualTo("GDuplicateFunc dup_func"));
			Assert.That(methodDecl.Parameters.ElementAt(3).ToCSharpString(_libraries), Is.EqualTo("IntPtr user_data"));
		});
	}

	[Test]
	public void g_str_tokenize_and_fold()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			gchar**
			g_str_tokenize_and_fold (
			  const gchar* string,
			  const gchar* translit_locale,
			  gchar*** ascii_alternates
			)"));

		var methodDecl = parser.ReadMethodDeclaration();
		methodDecl.Parameters.ElementAt(2).Comments = "Type: An array of gchar**\n\nThe argument will be set by the function.";
		methodDecl.ReturnTypeComments = "Type: An array of utf8";

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_str_tokenize_and_fold"));
			Assert.That(methodDecl.Parameters.First().ToCSharpString(_libraries), Is.EqualTo("string @string"));
			Assert.That(methodDecl.Parameters.ElementAt(1).ToCSharpString(_libraries), Is.EqualTo("string translit_locale"));
			Assert.That(methodDecl.Parameters.ElementAt(2).ToCSharpString(_libraries), Is.EqualTo("out string[] ascii_alternates"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("string[]"));
		});
	}

	[Test]
	public void g_unichar_islower()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			gboolean
			g_unichar_islower (
				gunichar c
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_unichar_islower"));
			Assert.That(methodDecl.Parameters.First().ToCSharpString(_libraries), Is.EqualTo("char c"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("bool"));
		});
	}

	[Test]
	public void g_test_trap_subprocess()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			void
			g_test_trap_subprocess (
			  const char* test_path,
			  guint64 usec_timeout,
			  GTestSubprocessFlags test_flags
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_test_trap_subprocess"));
			Assert.That(methodDecl.Parameters.ElementAt(0).ToCSharpString(_libraries), Is.EqualTo("string test_path"));
			Assert.That(methodDecl.Parameters.ElementAt(1).ToCSharpString(_libraries), Is.EqualTo("ulong usec_timeout"));
			Assert.That(methodDecl.Parameters.ElementAt(2).ToCSharpString(_libraries), Is.EqualTo("GTestSubprocessFlags test_flags"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("void"));
		});
	}

	[Test]
	public void g_test_init()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			void g_test_init (
				int* argc,
				char*** argv,
				...
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_test_init"));
			Assert.That(methodDecl.Parameters.ElementAt(0).ToCSharpString(_libraries), Is.EqualTo("ref int argc"));
			Assert.That(methodDecl.Parameters.ElementAt(1).ToCSharpString(_libraries), Is.EqualTo("ref string[] argv"));
			Assert.That(methodDecl.Parameters.ElementAt(2).ToCSharpString(_libraries), Is.EqualTo("IntPtr @__arglist"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("void"));
		});
	}

	[Test]
	public void g_file_get_contents()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			gboolean
			g_file_get_contents (
			  const gchar* filename,
			  gchar** contents,
			  gsize* length,
			  GError** error
			)"));

		var methodDecl = parser.ReadMethodDeclaration();
		methodDecl.Parameters.ElementAt(1).Comments = "The argument will be set by the function";
		methodDecl.Parameters.ElementAt(2).Comments = "The argument will be set by the function";
		methodDecl.Parameters.ElementAt(3).Comments = "The return location for";

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_file_get_contents"));
			Assert.That(methodDecl.Parameters.ElementAt(0).ToCSharpString(_libraries), Is.EqualTo("string filename"));
			Assert.That(methodDecl.Parameters.ElementAt(1).ToCSharpString(_libraries), Is.EqualTo("out string contents"));
			Assert.That(methodDecl.Parameters.ElementAt(2).ToCSharpString(_libraries), Is.EqualTo("out int length"));
			Assert.That(methodDecl.Parameters.ElementAt(3).ToCSharpString(_libraries), Is.EqualTo("out GErrorHandle error"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("bool"));
		});
	}

	[Test]
	public void g_atomic_ref_count_init()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			void
			g_atomic_ref_count_init (
			  gatomicrefcount* arc
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_atomic_ref_count_init"));
			Assert.That(methodDecl.Parameters.ElementAt(0).ToCSharpString(_libraries), Is.EqualTo("ref int arc"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("void"));
		});
	}

	[Test]
	public void g_log_writer_standard_streams()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			GLogWriterOutput
			g_log_writer_standard_streams (
			  GLogLevelFlags log_level,
			  const GLogField* fields,
			  gsize n_fields,
			  gpointer user_data
			)"));

		var methodDecl = parser.ReadMethodDeclaration();
		methodDecl.Parameters.ElementAt(1).Comments = "Type: An array of GLogField";

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_log_writer_standard_streams"));
			Assert.That(methodDecl.Parameters.ElementAt(0).ToCSharpString(_libraries), Is.EqualTo("GLogLevelFlags log_level"));
			Assert.That(methodDecl.Parameters.ElementAt(1).ToCSharpString(_libraries), Is.EqualTo("GLogField[] fields"));
			Assert.That(methodDecl.Parameters.ElementAt(2).ToCSharpString(_libraries), Is.EqualTo("int n_fields"));
			Assert.That(methodDecl.Parameters.ElementAt(3).ToCSharpString(_libraries), Is.EqualTo("IntPtr user_data"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("GLogWriterOutput"));
		});
	}

	[Test]
	public void g_clear_error()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			void
			g_clear_error (
				void
				GError** error
			)"));

		var methodDecl = parser.ReadMethodDeclaration();

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_clear_error"));
			Assert.That(methodDecl.Parameters.ElementAt(0).ToCSharpString(_libraries), Is.EqualTo("ref GErrorHandle error"));
		});
	}

	[Test]
	public void g_application_open()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			void
			g_application_open (
			  GApplication* application,
			  GFile** files,
			  gint n_files,
			  const gchar* hint
			)"));

		var methodDecl = parser.ReadMethodDeclaration();
		methodDecl.Parameters.ElementAt(1).Comments = "Type: An array of GFile*";

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_application_open"));
			Assert.That(methodDecl.Parameters.ElementAt(0).ToCSharpString(_libraries), Is.EqualTo("GApplicationHandle application"));
			Assert.That(methodDecl.Parameters.ElementAt(1).ToCSharpString(_libraries), Is.EqualTo("GFileHandle[] files"));
			Assert.That(methodDecl.Parameters.ElementAt(2).ToCSharpString(_libraries), Is.EqualTo("int n_files"));
			Assert.That(methodDecl.Parameters.ElementAt(3).ToCSharpString(_libraries), Is.EqualTo("string hint"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("void"));
		});
	}

	[Test]
	public void g_action_group_query_action()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			gboolean
			g_action_group_query_action (
			  GActionGroup* action_group,
			  const gchar* action_name,
			  gboolean* enabled,
			  const GVariantType** parameter_type,
			  const GVariantType** state_type,
			  GVariant** state_hint,
			  GVariant** state
			)"));

		var methodDecl = parser.ReadMethodDeclaration();
		methodDecl.Parameters.ElementAt(0).Comments = "";
		methodDecl.Parameters.ElementAt(1).Comments = "";
		methodDecl.Parameters.ElementAt(2).Comments = "The argument will be set by the function";
		methodDecl.Parameters.ElementAt(3).Comments = "The argument will be set by the function";
		methodDecl.Parameters.ElementAt(4).Comments = "The argument will be set by the function";
		methodDecl.Parameters.ElementAt(5).Comments = "The argument will be set by the function";
		methodDecl.Parameters.ElementAt(6).Comments = "The argument will be set by the function";

		Assert.Multiple(() =>
		{
			Assert.That(methodDecl.Name, Is.EqualTo("g_action_group_query_action"));
			Assert.That(methodDecl.Parameters.ElementAt(0).ToCSharpString(_libraries), Is.EqualTo("GActionGroupHandle action_group"));
			Assert.That(methodDecl.Parameters.ElementAt(1).ToCSharpString(_libraries), Is.EqualTo("string action_name"));
			Assert.That(methodDecl.Parameters.ElementAt(2).ToCSharpString(_libraries), Is.EqualTo("out bool enabled"));
			Assert.That(methodDecl.Parameters.ElementAt(3).ToCSharpString(_libraries), Is.EqualTo("out GVariantTypeHandle parameter_type"));
			Assert.That(methodDecl.Parameters.ElementAt(4).ToCSharpString(_libraries), Is.EqualTo("out GVariantTypeHandle state_type"));
			Assert.That(methodDecl.Parameters.ElementAt(5).ToCSharpString(_libraries), Is.EqualTo("out GVariantHandle state_hint"));
			Assert.That(methodDecl.Parameters.ElementAt(6).ToCSharpString(_libraries), Is.EqualTo("out GVariantHandle state"));
			Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("bool"));
		});
	}

	[Test]
	public void g_type_interfaces()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			GType*
			g_type_interfaces (
			  GType type,
			  guint* n_interfaces
			)"));

		var methodDecl = parser.ReadMethodDeclaration();
		methodDecl.ReturnTypeComments = "Type: An array of GType";
		Assert.That(methodDecl.ToCSharpReturnType(), Is.EqualTo("GType[]"));
	}

	[Test]
	public void Parameter_guint8Array_to_byteArray()
	{
		var parser = new CDeclParser(new CDeclLexer(@"const guint8* data"));
		var decl = parser.ReadMethodParameter();
		decl.Comments = "An array of guint8";
		Assert.That(decl.ToCSharpString(new List<LibraryDeclaration>()), Is.EqualTo("byte[] data"));
	}

	[Test]
	public void Parameter_gunichar_to_string()
	{
		var parser = new CDeclParser(new CDeclLexer(@"const gunichar2* str"));
		var paramDecl = parser.ReadMethodParameter();
		Assert.That(paramDecl.ToCSharpString(_libraries), Is.EqualTo("string str"));
	}
}
