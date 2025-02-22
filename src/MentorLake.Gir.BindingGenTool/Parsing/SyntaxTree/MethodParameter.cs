namespace BindingTransform;

public class MethodParameter
{
	public ParsedType Type { get; set; }
	public string Name { get; set; }
	public bool IsVarArgs { get; set; }
	public string Comments { get; set; } = "";
}
