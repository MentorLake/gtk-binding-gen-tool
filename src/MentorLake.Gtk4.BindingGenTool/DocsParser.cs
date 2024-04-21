using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Sgml;

namespace BindingTransform;

public class DocsParser
{
	public static MethodDeclaration ParseMethodHtml(string methodFilePath)
	{
		if (methodFilePath.Contains("https://"))  return null;
		if (methodFilePath.Contains("ftp:/"))  return null;
		var xml = LoadHtml(methodFilePath);
		if (xml.XPathSelectElement("body//h3[contains(text(), 'Function Macro')]") != null) return null;
		var decl = xml.XPathSelectElement("body//h4[contains(text(), 'Declaration')]/..//div[contains(@class, 'docblock')]").Value;
		var parameterComments = xml.XPathSelectElement("body//h4[contains(text(), 'Parameters')]/..//dl[@class='arguments']");
		var returnTypeComments = xml.XPathSelectElement("body//h4[contains(text(), 'Return value')]/..//div[@class='arg-description']")?.Value ?? "";
		var parser = new CDeclParser(new CDeclLexer(decl));
		var methodDecl = parser.ReadMethodDeclaration();
		methodDecl.ReturnTypeComments = returnTypeComments;

		foreach (var p in methodDecl.Parameters)
		{
			p.Comments = parameterComments?.XPathSelectElement($"//dt/code[text()='{p.Name}']/../following-sibling::dd")?.Value ?? "";
		}

		return methodDecl;
	}

	public static InterfaceDeclaration ParseInterfaceHtml(LibraryConfig config, string filePath)
	{
		var classDirectory = Path.GetDirectoryName(filePath);
		var xml = LoadHtml(filePath);
		var name = Path.GetFileNameWithoutExtension(filePath).Replace("iface.", "");
		var parentClass = xml.XPathSelectElement("body//h4[contains(text(), 'Prerequisite')]/..//code")?.Value ?? "";

		return new InterfaceDeclaration()
		{
			Name = config.DeclPrefix + name,
			ParentClassName = parentClass,
			Methods = xml
				.XPathSelectElements("body//div[@class='methods toggle-wrapper']/h4[contains(text(), 'Instance methods')]/..//div[@class='docblock']//h6//a")
				.Select(a => a.Attribute("href").Value)
				.Select(x => Path.Join(classDirectory, x))
				.Select(x => ParseMethodHtml(x))
				.ToList()
		};
	}

	public static ClassDeclaration ParseClassHtml(LibraryConfig config, string filePath)
	{
		var classDirectory = Path.GetDirectoryName(filePath);
		var xml = LoadHtml(filePath);
		var classDefLines = xml.XPathSelectElement("body//section/summary//pre/code")?.Value?.Split("\n");
		var name = config.DeclPrefix + Path.GetFileNameWithoutExtension(filePath).Replace("class.", "");
		var parentClass = xml.XPathSelectElement("body//h4[contains(text(), 'Ancestors')]/..//ul/li/a")?.Value ?? "";

		if (string.IsNullOrEmpty(parentClass) && classDefLines[0].Contains(":"))
		{
			parentClass = classDefLines[0].Replace("{", "").Split(":").Last();
		}

		var classDecl = new ClassDeclaration()
		{
			Name = name,
			ParentClassName = parentClass,
			Constructors = xml
				.XPathSelectElements("body//h4[@id='constructors']/..//div[@class='docblock']//a")
				.Select(a => a.Attribute("href").Value)
				.Select(x => Path.Join(classDirectory, x))
				.Select(x => ParseMethodHtml(x))
				.Where(x => x != null)
				.ToList(),
			Interfaces = xml.XPathSelectElements("body//h4[@id='implements']/..//div[@class='docblock']//a")
				.Select(a => a.Value)
				.ToList(),
			Signals = xml
				.XPathSelectElements("body//h4[contains(text(), 'Signals')]/../div[@class='docblock']//a")
				.Select(a => a.Attribute("href").Value)
				.Select(x => Path.Join(classDirectory, x))
				.Select(x => ParseMethodHtml(x))
				.Where(x => x != null)
				.ToList(),
			Functions = xml
				.XPathSelectElements("body//h4[contains(text(), 'Functions')]/../div[@class='docblock']/div//a")
				.Select(a => a.Attribute("href").Value)
				.Select(x => Path.Join(classDirectory, x))
				.Where(f => File.Exists(f))
				.Select(x => ParseMethodHtml(x))
				.ToList(),
			Methods = xml
				.XPathSelectElements("body//div[@class='toggle-wrapper methods']/h4[contains(text(), 'Instance methods')]/../div[@class='docblock']/div")
				.Select(m => m.XPathSelectElement(".//a"))
				.Select(a => a.Attribute("href").Value)
				.Select(x => Path.Join(classDirectory, x))
				.Select(x => ParseMethodHtml(x))
				.ToList()
		};

		return classDecl;
	}

	public static MethodDeclaration ParseCallbackHtml(string callbackFilePath)
	{
		var xml = LoadHtml(callbackFilePath);
		var decl = xml.XPathSelectElement("body//h4[contains(text(), 'Declaration')]/..//pre//code//pre").Value;
		var parameterComments = xml.XPathSelectElement("body//h4[contains(text(), 'Parameters')]/..//dl[@class='arguments']");
		var returnValueComments = xml.XPathSelectElement("body//h4[contains(text(), 'Return value')]/..//div[@class='arg-description']")?.Value ?? "";
		var parser = new CDeclParser(new CDeclLexer(decl));
		var callbackDecl = parser.ReadFunctionPointer();
		callbackDecl.ReturnTypeComments = returnValueComments;

		foreach (var p in callbackDecl.Parameters)
		{
			p.Comments = parameterComments?.XPathSelectElement($"//dt/code[text()='{p.Name}']/../following-sibling::dd")?.Value ?? "";
		}

		return callbackDecl;
	}

	public static StructDeclaration ParseStruct(string filePath)
	{
		var xml = LoadHtml(filePath);
		var parser = new CDeclParser(new CDeclLexer(xml.XPathSelectElement("body//section/summary//pre/code").Value));
		var decl = parser.ReadStructDeclaration();
		var parameterComments = xml.XPathSelectElement("body//h6[contains(text(), 'Structure members')]/..//div[@class='docblock']");
		var structDirectory = Path.GetDirectoryName(filePath);

		decl.Constructors = xml
			.XPathSelectElements("body//h4[@id='constructors']/..//div[@class='docblock']//a")
			.Select(a => a.Attribute("href").Value)
			.Select(x => Path.Join(structDirectory, x))
			.Select(x => ParseMethodHtml(x))
			.Where(x => x != null)
			.ToList();

		foreach (var p in decl.Properties)
		{
			p.Comments = parameterComments?.XPathSelectElement($"//dt/code[text()='{p.Name}']/../following-sibling::dd")?.Value ?? "";
		}

		return decl;
	}

	private static XElement LoadHtml(string path)
	{
		var fileReader = new StringReader(File.ReadAllText(path));
		var sgmlReader = new SgmlReader()
		{
			DocType = "HTML",
			WhitespaceHandling = WhitespaceHandling.All,
			CaseFolding = CaseFolding.ToLower,
			InputStream = fileReader,
			IgnoreDtd = true
		};

		return XElement.Load(sgmlReader);
	}

	public static AliasDeclaration ParseAlias(string filePath)
	{
		var xml = LoadHtml(filePath);
		var decl = xml.XPathSelectElement("body//h4[contains(text(), 'Description')]/..//pre//code").Value;
		var parser = new CDeclParser(new CDeclLexer(decl));
		return parser.ReadAlias();
	}

	public static EnumDeclaration ParseEnumHtml(LibraryConfig config, string filePath)
	{
		var xml = LoadHtml(filePath);
		var enumDeclaration = new EnumDeclaration();
		enumDeclaration.Name = config.DeclPrefix + Path.GetFileNameWithoutExtension(filePath).Replace("enum.", "").Replace("flags.", "").Replace("error.", "");

		var enumMembers = xml.XPathSelectElements("body//table[@class='enum-members']//td/a").ToList();

		if (!enumMembers.Any())
		{
			enumMembers = xml.XPathSelectElements("body//dl[@class='enum-members']//dt/code").ToList();
		}

		enumDeclaration.Values = enumMembers.Select(e => e.Value).ToList();
		return enumDeclaration;
	}
}
