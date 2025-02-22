namespace BindingTransform;

public class MethodDeclaration
{
	public ParsedType ReturnType { get; set; }
	public string ReturnTypeComments { get; set; } = "";
	public string Name { get; set; }
	public List<MethodParameter> Parameters { get; set; } = new();
	public bool IsReturnDataOwnedByInstance => ReturnTypeComments.Contains("The returned data is owned by the instance") || ReturnTypeComments.Contains("The data is owned by the called function.");

	public override string ToString() => Name;
}
