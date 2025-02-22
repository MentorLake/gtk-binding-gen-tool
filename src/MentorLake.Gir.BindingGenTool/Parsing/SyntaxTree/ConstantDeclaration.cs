namespace BindingTransform;

public class ConstantDeclaration
{
	public string Name { get; set; }
	public string Value { get; set; }

	public override string ToString() => $"{Name} = {Value}";
}
