using System.Text;
using MentorLake.Gir.Core;

namespace BindingTransform.Serialization.Gir;

public class GirLibrarySerializer(List<Repository> repositories)
{
	private Namespace _currentNamespace;

	public string SerializeClass(ConvertedClass c)
	{
		var output = new StringBuilder();
		output.AppendLine($"public class {c.Name}{SerializeInherited(c)}");
		output.AppendLine("{");
		foreach (var m in c.Constructors) output.AppendLine(SerializeConstructor(m, c.Name));
		foreach (var m in c.Functions) output.AppendLine(SerializeMethod(m, c.Name));
		output.AppendLine("}");

		if (c.Signals.Any())
		{
			//output.AppendLine().AppendLine(CSharpSignalsSerializer.Serialize(c, libraryDeclaration, libraries));
		}

		output.AppendLine();
		output.AppendLine($"public static class {c.Name}Extensions");
		output.AppendLine("{");
		foreach (var m in c.Methods.DistinctBy(m => m.Name)) output.AppendLine(SerializeMethod(m, c.Name));
		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"internal class {c.Name}Externs");
		output.AppendLine("{");
		foreach (var m in c.Constructors) output.AppendLine(SerializeExternMethod(m));
		foreach (var m in c.Methods.Concat(c.Functions).DistinctBy(m => m.Name)) output.AppendLine(SerializeExternMethod(m));
		output.AppendLine("}");
		return output.ToString();
	}

	private string SerializeInherited(ConvertedClass c)
	{
		var parentClassName = "";

		if (!string.IsNullOrEmpty(c.Parent))
		{
			parentClassName = c.Parent.Contains(".") ? c.Parent.Split(".")[1].Trim() : c.Parent;
		}

		var inherited = new List<string>();
		inherited.Add(parentClassName);
		inherited.AddRange(c.Implements);
		inherited = inherited.Where(i => !string.IsNullOrEmpty(i)).ToList();
		if (inherited.Any()) return " : " + string.Join(", ", inherited);
		return "";
	}

	private string SerializeType(ConvertedType t)
	{
		var typeName = "MentorLake." + t.Namespace + "." + t.ConvertedTypeName;
		if (t.IsBuiltInType) typeName = t.ConvertedTypeName;
		return typeName;
	}

	private string SerializeParameter(ConvertedParameter p, bool isMarshalled = false, bool isInstanceMethod = false)
	{
		var attr = "";
		var typeName = SerializeType(p.ConvertedType);

		if (p.ConvertedType.IsSafeHandle && isMarshalled)
		{
			var marshalledHandleType = p.ConvertedType.IsInterface ? typeName + "Impl" : typeName;

			if (!p.ConvertedType.IsArray)
			{
				attr = $"[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DelegateSafeHandleMarshaller<{marshalledHandleType}>))]";
			}
			else
			{
				marshalledHandleType = marshalledHandleType.Replace("[]", "");
				var sizeParamIndex = isInstanceMethod ? p.ConvertedType.ArraySizeIndex + 1 : p.ConvertedType.ArraySizeIndex;
				var args = new List<string>();
				args.Add("UnmanagedType.LPArray");
				args.Add("ArraySubType = UnmanagedType.Struct");
				if (sizeParamIndex != -1) args.Add($"SizeParamIndex = {sizeParamIndex}");
				args.Add($"MarshalTypeRef = typeof(DelegateSafeHandleMarshaller<{marshalledHandleType}>)");
				attr = $"[MarshalAs({string.Join(", ", args)})]";
			}
		}

		return string.Join(" ", new[] { attr, p.Modifier, typeName, p.Name }.Where(s => !string.IsNullOrEmpty(s)));
	}

	private string SerializeCallback(ConvertedCallback cb)
	{
		var output = new StringBuilder();
		output.AppendLine("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
		var parameters = cb.Parameters == null ? "" : string.Join(", ", cb.Parameters.Select(p => SerializeParameter(p, true)));
		output.AppendLine($"public delegate {SerializeType(cb.ReturnValue.Type)} {cb.Name.NormalizeName()}({parameters});");
		return output.ToString();
	}

	private string SerializeAlias(ConvertedAlias alias)
	{
		var output = new StringBuilder();
		output.Append($"public struct {alias.Name}");
		output.AppendLine();
		output.AppendLine("{");
		if (alias.WrappedType.ConvertedTypeName != "void") output.AppendLine($"\tpublic {SerializeType(alias.WrappedType)} Value;");
		output.AppendLine("}");
		output.AppendLine();
		output.AppendLine($"public class {alias.Name}Handle : BaseSafeHandle");
		output.AppendLine("{");
		output.AppendLine("}");
		return output.ToString();
	}

	private string SerializeConstructor(ConvertedMethod ctor, string className)
	{
		var parameters = string.Join(", ", ctor.Parameters.Select(p => SerializeParameter(p)));
		var methodName = ctor.Name.ToPascalCase().Replace(className, "");
		var externCall = $"{className}Externs.{ctor.ExternName}({string.Join(", ", ctor.Parameters.Select(p => $"{p.Modifier} {p.Name}".Trim()))});";
		var output = new StringBuilder();
		output.AppendLine($"\tpublic static {className} {methodName}({parameters})");
		output.AppendLine("\t{");
		output.AppendLine($"\t\treturn {externCall}");
		output.AppendLine("\t}");
		return output.ToString();
	}

	private string SerializeMethod(ConvertedMethod m, string className, bool allowGenerics = true)
	{
		var methodName = m.Name.ToPascalCase().Replace(className, "");
		var externCall = $"{className}Externs.{m.ExternName}({string.Join(", ", m.Parameters.Select(p => $"{p.Modifier} {p.Name}".Trim()))});";
		var returnType = SerializeType(m.ReturnValue.Type);
		var output = new StringBuilder();

		if (m.IsInstanceMethod)
		{
			var instanceParam = m.Parameters.First();
			var otherSerializedParams = m.Parameters.Skip(1).Select(p => SerializeParameter(p)).ToList();

			if (returnType == "void" && allowGenerics)
			{
				var serializedInstanceParams = $"this T {instanceParam.Name.NormalizeName()}";
				var allSerializedParams = string.Join(", ", new List<string>() { serializedInstanceParams }.Concat(otherSerializedParams));
				output.AppendLine($"\tpublic static T {methodName}<T>({allSerializedParams}) where T : {className}");
				output.AppendLine("\t{");
				output.AppendLine($"\t\t{externCall}");
				output.AppendLine($"\t\treturn {instanceParam.Name.NormalizeName()};");
				output.AppendLine("\t}");
			}
			else if (returnType == "void")
			{
				var serializedInstanceParams = $"this {SerializeType(instanceParam.ConvertedType)} {instanceParam.Name.NormalizeName()}";
				var allSerializedParams = string.Join(", ", new List<string>() { serializedInstanceParams }.Concat(otherSerializedParams));
				output.AppendLine($"\tpublic static void {methodName}({allSerializedParams})");
				output.AppendLine("\t{");
				output.AppendLine($"\t\t{externCall}");
				output.AppendLine("\t}");
			}
			else
			{
				var serializedInstanceParams = $"this {SerializeType(instanceParam.ConvertedType)} {instanceParam.Name.NormalizeName()}";
				var allSerializedParams = string.Join(", ", new List<string>() { serializedInstanceParams }.Concat(otherSerializedParams));
				output.AppendLine($"\tpublic static {returnType} {methodName}({allSerializedParams})");
				output.AppendLine("\t{");
				output.AppendLine($"\t\treturn {externCall}");
				output.AppendLine("\t}");
			}
		}
		else if (returnType == "void")
		{
			var parameters = string.Join(", ", m.Parameters.Select(p => SerializeParameter(p)));
			output.AppendLine($"\tpublic static void {methodName}({parameters})");
			output.AppendLine("\t{");
			output.AppendLine($"\t\t{externCall}");
			output.AppendLine("\t}");
		}
		else
		{
			var parameters = string.Join(", ", m.Parameters.Select(p => SerializeParameter(p)));
			output.AppendLine($"\tpublic static {returnType} {methodName}({parameters})");
			output.AppendLine("\t{");
			output.AppendLine($"\t\treturn {externCall}");
			output.AppendLine("\t}");
		}
		return output.ToString();
	}

	private const string CustomStringMarshallerAttribute = "[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NoNativeFreeStringMarshaller))]";

	private string SerializeExternMethod(ConvertedMethod m)
	{
		var output = new StringBuilder();
		output.AppendLine($"\t[DllImport({_currentNamespace.Name}Library.Name)]");
		if (m.TransferOwnership == ReturnValueTransferOwnership.Full && m.ReturnValue.Type.ConvertedTypeName == "string") output.AppendLine("\t" + CustomStringMarshallerAttribute);
		var parameters = string.Join(", ", m.Parameters.Select(p => SerializeParameter(p, true, m.IsInstanceMethod)));
		output.AppendLine($"\tinternal static extern {SerializeType(m.ReturnValue.Type)} {m.ExternName}({parameters});");
		return output.ToString();
	}

	private string SerializeField(ConvertedField field)
	{
		var type = "";
		if (field.Callback != null) type = "IntPtr";
		else if (field.Type != null) type = field.Type.IsPointer ? "IntPtr" : field.Type.ConvertedTypeName;
		else throw new Exception("Unknown field type: " + field.Name);
		return $"public {type} {field.Name};";
	}

	private string SerializeUnion(ConvertedUnion union, string nameOverride = "")
	{
		var output = new StringBuilder();
		var unionName = string.IsNullOrEmpty(nameOverride) ? union.Name : nameOverride;

		foreach (var inner in union.Records ?? new()) output.AppendLine(SerializeUnion(inner, unionName + "_" + inner.Name));

		output.AppendLine($"public class {unionName}Handle : BaseSafeHandle");
		output.AppendLine("{");
		foreach (var constructor in union.Constructors) output.AppendLine(SerializeConstructor(constructor, union.Name));
		output.AppendLine("}");
		output.AppendLine();

		output.AppendLine();
		output.AppendLine($"public static class {unionName}Extensions");
		output.AppendLine("{");
		foreach (var m in union.Methods) output.AppendLine(SerializeMethod(m, unionName, false));
		output.AppendLine("}");

		output.AppendLine($"internal class {unionName}Externs");
		output.AppendLine("{");
		foreach (var m in union.Constructors) output.AppendLine(SerializeExternMethod(m));
		foreach (var m in union.Methods) output.AppendLine(SerializeExternMethod(m));
		foreach (var m in union.Functions) output.AppendLine(SerializeExternMethod(m));
		output.AppendLine("}");
		output.AppendLine();

		output.Append($"public struct {unionName}");
		output.AppendLine();
		output.AppendLine("{");
		foreach (var r in union.Records ?? new()) output.AppendLine($"\tpublic {unionName + "_" + r.Name} {r.Name};");
		foreach (var f in union.Fields) output.AppendLine($"\t{SerializeField(f)}");
		foreach (var m in union.Functions) output.AppendLine(SerializeMethod(m, unionName, false));
		output.AppendLine("}");

		return output.ToString();
	}

	private string SerializeBitfield(ConvertedBitField field)
	{
		var output = new StringBuilder();
		output.AppendLine("[Flags]");
		output.AppendLine($"public enum {field.Name}");
		output.AppendLine("{");

		for (var i = 0; i < field.Members.Count; i++)
		{
			var member = field.Members[i];
			output.Append("\t" + member.Key + " = " + member.Value);
			if (i < field.Members.Count - 1) output.Append(",");
			output.AppendLine();
		}

		output.AppendLine("}");
		return output.ToString();
	}

	private string SerializeEnumeration(ConvertedEnumeration enumeration)
	{
		var output = new StringBuilder();
		output.AppendLine("[Flags]");
		output.AppendLine($"public enum {enumeration.Name}");
		output.AppendLine("{");

		for (var i = 0; i < enumeration.Members.Count; i++)
		{
			var member = enumeration.Members[i];
			output.Append("\t" + member.Key + " = " + member.Value);
			if (i < enumeration.Members.Count - 1) output.Append(",");
			output.AppendLine();
		}

		output.AppendLine("}");
		return output.ToString();
	}

	private string SerializeInterface(ConvertedInterface s)
	{
		var output = new StringBuilder();
		output.AppendLine($"public interface {s.Name}");
		output.AppendLine("{");
		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"internal class {s.Name}Impl : BaseSafeHandle, {s.Name}");
		output.AppendLine("{");
		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"public static class {s.Name}Extensions");
		output.AppendLine("{");
		foreach (var m in s.Methods.Concat(s.Functions)) output.AppendLine(SerializeMethod(m, s.Name));
		output.AppendLine("}");
		output.AppendLine();
		output.AppendLine($"internal class {s.Name}Externs");
		output.AppendLine("{");

		foreach (var m in s.Methods.Concat(s.Functions))
		{
			output.AppendLine(SerializeExternMethod(m));
		}

		output.AppendLine("}");
		return output.ToString();
	}

	public void WriteAllFiles(string outputBaseDirectory, Repository repo)
	{
		_currentNamespace = repo.Namespace.First();
		var converter = new GirConverter(_currentNamespace, repositories);
		var outputDir = Path.Join(outputBaseDirectory, _currentNamespace.Name);
		if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
		Directory.CreateDirectory(outputDir);

		var header = $"namespace MentorLake.{_currentNamespace.Name};";

		var libraryNameFile = $"{header}\r\n\r\npublic static class {_currentNamespace.Name}Library {{ public const string Name = \"{_currentNamespace.SharedLibrary}\"; }}";
		File.WriteAllText(Path.Join(outputDir, $"{_currentNamespace.Name}Library.cs"), libraryNameFile);

		foreach (var cb in _currentNamespace.Callback)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeCallback(converter.ConvertCallback(cb)));
			File.WriteAllText(Path.Join(outputDir, cb.Type + ".cs"), output.ToString());
		}

		foreach (var s in _currentNamespace.Alias)
		{
			var output = new StringBuilder();
			var alias = converter.ConvertAlias(s);
			output.AppendLine(header).AppendLine().Append(SerializeAlias(alias));
			File.WriteAllText(Path.Join(outputDir, alias.Name + ".cs"), output.ToString());
		}

		foreach (var s in _currentNamespace.Record)
		{
			var output = new StringBuilder();
			var record = converter.ConvertRecord(s);
			output.AppendLine(header).AppendLine().Append(SerializeUnion(record));
			File.WriteAllText(Path.Join(outputDir, record.Name + ".cs"), output.ToString());
		}

		foreach (var s in _currentNamespace.Union)
		{
			var output = new StringBuilder();
			var union = converter.ConvertUnion(s);
			if (string.IsNullOrEmpty(union.Name))
			{

			}
			output.AppendLine(header).AppendLine().Append(SerializeUnion(union));
			File.WriteAllText(Path.Join(outputDir, union.Name + ".cs"), output.ToString());
		}

		foreach (var s in _currentNamespace.Bitfield)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeBitfield(converter.ConvertBitField(s)));
			File.WriteAllText(Path.Join(outputDir, s.Type + ".cs"), output.ToString());
		}

		foreach (var s in _currentNamespace.Enumeration)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeEnumeration(converter.ConvertEnumeration(s)));
			File.WriteAllText(Path.Join(outputDir, s.Type + ".cs"), output.ToString());
		}

		foreach (var s in _currentNamespace.Interface)
		{
			var output = new StringBuilder();
			var i = converter.ConvertInterface(s);
			output.AppendLine(header).AppendLine().Append(SerializeInterface(i));
			File.WriteAllText(Path.Join(outputDir, i.Name + ".cs"), output.ToString());
		}

		foreach (var s in _currentNamespace.Class)
		{
			var output = new StringBuilder();
			var c = converter.ConvertClass(s);
			output.AppendLine(header).AppendLine().Append(SerializeClass(c));
			File.WriteAllText(Path.Join(outputDir, $"{c.Name}.cs"), output.ToString());
		}

		var convertedGlobalFunctions = _currentNamespace.Function == null ? new() : _currentNamespace.Function.Select(converter.ConvertFunction).ToList();
		var globalFunctionsOutput = new StringBuilder();
		globalFunctionsOutput.AppendLine(header);
		globalFunctionsOutput.AppendLine();
		globalFunctionsOutput.AppendLine($"public class {_currentNamespace.Name}GlobalFunctions");
		globalFunctionsOutput.AppendLine("{");
		foreach (var f in convertedGlobalFunctions) globalFunctionsOutput.AppendLine(SerializeMethod(f, _currentNamespace.Name + "GlobalFunctions"));
		globalFunctionsOutput.AppendLine("}");
		globalFunctionsOutput.AppendLine();
		globalFunctionsOutput.AppendLine($"internal class {_currentNamespace.Name}GlobalFunctionsExterns");
		globalFunctionsOutput.AppendLine("{");
		foreach (var f in convertedGlobalFunctions) globalFunctionsOutput.AppendLine(SerializeExternMethod(f));
		globalFunctionsOutput.AppendLine("}");
		File.WriteAllText(Path.Join(outputDir, $"{_currentNamespace.Name}GlobalFunctions.cs"), globalFunctionsOutput.ToString());

		// var constants = new StringBuilder();
		// constants.AppendLine(header);
		// constants.AppendLine();
		// constants.AppendLine($"public class {repo.Name}Constants");
		// constants.AppendLine("{");
		//
		// foreach (var c in repo.Constants)
		// {
		// 	constants.AppendLine("\t" + CSharp.CSharpConstantSerializer.Serialize(c, repo, repos));
		// }
		//
		// constants.AppendLine("}");
		// File.WriteAllText(Path.Join(outputDir, $"{ns.Name}Constants.cs"), constants.ToString());
	}
}
