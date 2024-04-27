using System.Diagnostics;
using BindingTransform;
using BindingTransform.Serialization.CSharp;

namespace MentorLake.Gtk4.BindingGenTool.Tests;

public class DelegateTests
{
	[Test]
	public void GDBusSubtreeDispatchFunc()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			const GDBusInterfaceVTable*
			(* GDBusSubtreeDispatchFunc) (
			  GDBusConnection* connection,
			  const gchar* sender,
			  const gchar* object_path,
			  const gchar* interface_name,
			  const gchar* node,
			  gpointer* out_user_data,
			  gpointer user_data
			)"));

		var methodDecl = parser.ReadFunctionPointer();
		methodDecl.Parameters.ElementAt(5).Comments = "Return location for user data to pass to functions in the returned GDBusInterfaceVTable";
		var decl = methodDecl.ToCSharpDecl(new List<LibraryDeclaration>());

		Console.WriteLine(decl);

		Assert.Multiple(() =>
		{
			Assert.That(decl, Is.EqualTo("GDBusInterfaceVTableHandle GDBusSubtreeDispatchFunc(GDBusConnectionHandle connection, string sender, string object_path, string interface_name, string node, out IntPtr out_user_data, IntPtr user_data)"));
		});
	}

	[Test]
	public void GDBusSubtreeIntrospectFunc()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			GDBusInterfaceInfo**
			(* GDBusSubtreeIntrospectFunc) (
			  GDBusConnection* connection,
			  const gchar* sender,
			  const gchar* object_path,
			  const gchar* node,
			  gpointer user_data
			)"));

		var methodDecl = parser.ReadFunctionPointer();
		methodDecl.ReturnTypeComments = "Type: An array of GDBusInterfaceInfo*";
		var decl = methodDecl.ToCSharpDecl(new List<LibraryDeclaration>());

		Console.WriteLine(decl);

		Assert.Multiple(() =>
		{
			Assert.That(decl, Is.EqualTo("GDBusInterfaceInfoHandle[] GDBusSubtreeIntrospectFunc(GDBusConnectionHandle connection, string sender, string object_path, string node, IntPtr user_data)"));
		});
	}
}
