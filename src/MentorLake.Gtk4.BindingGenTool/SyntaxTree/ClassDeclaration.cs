namespace BindingTransform;

public class ClassDeclaration
{
	public string Name { get; set; }
	public string ParentClassName { get; set; }
	public List<string> Interfaces { get; set; } = new();
	public List<MethodDeclaration> Methods { get; set; } = new();
	public List<MethodDeclaration> Constructors { get; set; } = new();
	public List<MethodDeclaration> Signals { get; set; }
	public List<MethodDeclaration> Functions { get; set; }
}
