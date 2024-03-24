namespace BindingTransform;

public class InterfaceDeclaration
{
	public string Name { get; set; }
	public List<MethodDeclaration> Methods { get; set; }
	public string ParentClassName { get; set; }
}
