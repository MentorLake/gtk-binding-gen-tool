using BindingTransform;

namespace MentorLake.Gtk4.BindingGenTool.Tests;

public class StructTests
{
	[Test]
	public void GMarkupParser()
	{
		var parser = new CDeclParser(new CDeclLexer(@"
			struct GMarkupParser {
			  void (* start_element) (
			    GMarkupParseContext* context,
			    const gchar* element_name,
			    const gchar** attribute_names,
			    const gchar** attribute_values,
			    gpointer user_data,
			    GError** error
			  );
			}"));

		var s = parser.ReadStructDeclaration();
	}
}
