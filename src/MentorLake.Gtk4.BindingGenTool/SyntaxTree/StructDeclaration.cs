namespace BindingTransform;

public class StructDeclaration
{
	public string Name { get; set; }
	public List<StructProperty> Properties { get; set; } = new();
}
