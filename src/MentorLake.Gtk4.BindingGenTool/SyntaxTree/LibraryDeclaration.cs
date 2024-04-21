namespace BindingTransform;

public class LibraryDeclaration
{
	public string Name { get; set; }
	public List<ClassDeclaration> Classes { get; set; }
	public List<MethodDeclaration> Delegates { get; set; }
	public List<StructDeclaration> Structs { get; set; }
	public List<AliasDeclaration> Aliases { get; set; }
	public List<EnumDeclaration> Enums { get; set; }
	public List<EnumDeclaration> Errors { get; set; }
	public List<EnumDeclaration> Flags { get; set; }
	public List<StructDeclaration> Unions { get; set; }
	public List<MethodDeclaration> Functions { get; set; }
	public List<InterfaceDeclaration> Interfaces { get; set; }
	public LibraryConfig Config { get; set; }
}
