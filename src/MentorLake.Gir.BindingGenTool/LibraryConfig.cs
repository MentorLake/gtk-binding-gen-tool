namespace BindingTransform;

public class LibraryConfig
{
	public string Namespace { get; set; }
	public string DirectoryName { get; set; }
	public string DeclPrefix { get; set; }
	public List<string> TypesToSkip { get; set; } = new();
	public string SourceNamespace { get; set; }
}
