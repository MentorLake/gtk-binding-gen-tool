namespace BindingTransform;

public class StructProperty
{
	public string Name { get; set; }
	public ParsedType Type { get; set; }
	public MethodDeclaration Func { get; set; }
	public int Bits { get; set; }
	public string Comments { get; set; }
	public bool IsArray => Comments.Contains("A pointer to a NULL-terminated array");
}
