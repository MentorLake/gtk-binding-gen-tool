using System.Text;
using MentorLake.Gir.Core;

namespace BindingTransform.Serialization.Gir;

public class GirLibrarySerializer
{
	private ConvertedNamespace _currentNamespace;
	private List<ConvertedNamespace> _allNamespaces;

	public string SerializeClass(ConvertedClass c)
	{
		var output = new StringBuilder();
		var partialKeyword = c.Name == "GObjectHandle" ? "partial " : "";

		output.AppendLine($"public {partialKeyword}class {c.Name}{SerializeInherited(c)}");
		output.AppendLine("{");
		foreach (var m in c.Constructors) output.AppendLine(SerializeConstructor(m, c.Name));
		foreach (var m in c.Functions) output.AppendLine(SerializeMethod(m, c.Name));
		output.AppendLine("}");

		var allSignals = c.Signals.ToList();

		if (c.Implements.Any())
		{
			var allInterfaces = _allNamespaces.SelectMany(n => n.Interfaces).ToList();

			foreach (var implementedInterfaceName in c.Implements)
			{
				var i = allInterfaces.First(i => i.Name == implementedInterfaceName);
				allSignals.AddRange(i.Signals);
			}
		}

		if (allSignals.Any())
		{
			output.AppendLine(SerializeSignals(c, allSignals));
		}

		output.AppendLine();
		output.AppendLine($"public static class {c.Name}Extensions");
		output.AppendLine("{");
		foreach (var m in c.Methods.DistinctBy(m => m.Name)) output.AppendLine(SerializeMethod(m, c.Name));
		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"internal class {c.Name}Externs");
		output.AppendLine("{");
		foreach (var m in c.Constructors) output.AppendLine(SerializeExternMethod(m, true));
		foreach (var m in c.Methods.Concat(c.Functions).DistinctBy(m => m.Name)) output.AppendLine(SerializeExternMethod(m, m.TransferOwnership == ReturnValueTransferOwnership.Full && IsGObjectHandle(m.ReturnValue.Type.CSharpTypeName)));
		output.AppendLine("}");
		return output.ToString();
	}

	private string SerializeSignals(ConvertedInterface c, List<ConvertedSignal> allSignals)
	{
		var output = new StringBuilder();
		output.AppendLine($"public static class {c.Name}SignalExtensions");
		output.AppendLine("{");

		foreach (var signal in allSignals)
		{
			var handlerReturn = signal.ReturnValue.Type.CSharpTypeName != "void" ? "signalStruct.ReturnValue" : "";
			var outParameterDefaultAssignments = string.Join("\n\t\t\t", signal.Parameters.Where(p => p.Modifier == "out").Select(p => $"{p.Name} = default;"));

			var method = @$"
	public static IObservable<{c.Name}SignalStructs.{signal.Name.ToPascalCase()}Signal> Signal_{signal.Name.ToPascalCase()}(this {c.Name} instance, GConnectFlags connectFlags = GConnectFlags.G_CONNECT_AFTER)
	{{
		return Observable.Create((IObserver<{c.Name}SignalStructs.{signal.Name.ToPascalCase()}Signal> obs) =>
		{{
			{c.Name}SignalDelegates.{signal.Name.NormalizeName()} handler = ({string.Join(", ", signal.Parameters.Select(p => $"{p.Modifier} {SerializeType(p.ConvertedType)} {p.Name}"))}) =>
			{{
				{outParameterDefaultAssignments}

				var signalStruct = new {c.Name}SignalStructs.{signal.Name.ToPascalCase()}Signal()
				{{
					{string.Join(", ", signal.Parameters.Select(p => $"{p.Name.ToPascalCase()} = {p.Name}"))}
				}};

				obs.OnNext(signalStruct);
				return {handlerReturn};
			}};

			var gcHandle = System.Runtime.InteropServices.GCHandle.Alloc(handler);
			var handlerId = GObjectGlobalFunctions.SignalConnectData(instance, ""{signal.Name}"", Marshal.GetFunctionPointerForDelegate(handler), IntPtr.Zero, null, connectFlags);

			return Disposable.Create(() =>
			{{
				GObjectGlobalFunctions.SignalHandlerDisconnect(instance, handlerId);
				obs.OnCompleted();
				gcHandle.Free();
			}});
		}});
	}}";
			output.AppendLine(method);
		}

		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"public static class {c.Name}SignalStructs");
		output.AppendLine("{");
		foreach (var s in allSignals)
		{
			output.AppendLine();
			output.AppendLine($"public class {s.Name.ToPascalCase()}Signal");
			output.AppendLine("{");

			foreach (var p in s.Parameters)
			{
				output.AppendLine($"\tpublic {SerializeType(p.ConvertedType)} {p.Name.ToPascalCase()};");
			}

			if (s.ReturnValue.Type.CSharpTypeName != "void")
			{
				output.AppendLine($"\tpublic {SerializeType(s.ReturnValue.Type)} ReturnValue;");
			}

			output.AppendLine("}");
		}

		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"public static class {c.Name}SignalDelegates");
		output.AppendLine("{");

		foreach (var s in allSignals)
		{
			output.AppendLine();
			output.AppendLine(SerializeCallback(s));
		}

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
		var typeName = "MentorLake." + t.Namespace + "." + t.CSharpTypeName;
		if (t.IsBuiltInType) typeName = t.CSharpTypeName;
		return typeName;
	}

	private string SerializeParameter(ConvertedParameter p, bool isMarshalled = false, bool isInstanceMethod = false)
	{
		var attr = "";
		var typeName = SerializeType(p.ConvertedType);

		if (p.ConvertedType.IsSafeHandle && isMarshalled)
		{
			var marshalledHandleType = p.ConvertedType.IsInterface ? typeName + "Impl" : typeName;

			if (!p.ConvertedType.IsBasicArray)
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
		else if (p.ConvertedType.IsBasicArray && isMarshalled && p.Modifier == "out")
		{
			var sizeParamIndex = isInstanceMethod ? p.ConvertedType.ArraySizeIndex + 1 : p.ConvertedType.ArraySizeIndex;
			var args = new List<string>();
			args.Add("UnmanagedType.LPArray");
			if (sizeParamIndex != -1) args.Add($"SizeParamIndex = {sizeParamIndex}");
			attr = $"[MarshalAs({string.Join(", ", args)})]";
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
		if (alias.WrappedType.CSharpTypeName != "void") output.AppendLine($"\tpublic {SerializeType(alias.WrappedType)} Value;");
		output.AppendLine("}");
		output.AppendLine();
		output.AppendLine($"public class {alias.Name}Handle : BaseSafeHandle");
		output.AppendLine("{");
		output.AppendLine("}");
		output.AppendLine();
		output.AppendLine($"public static class {alias.Name}HandleExtensions");
		output.AppendLine("{");
		output.AppendLine($"\tpublic static {alias.Name} Dereference(this {alias.Name}Handle x) => System.Runtime.InteropServices.Marshal.PtrToStructure<{alias.Name}>(x.DangerousGetHandle());");
		if (alias.WrappedType.CSharpTypeName != "void") output.AppendLine($"\tpublic static {SerializeType(alias.WrappedType)} DereferenceValue(this {alias.Name}Handle x) => x.Dereference().Value;");
		output.AppendLine("}");
		return output.ToString();
	}

	private string SerializeConstructor(ConvertedMethod ctor, string className)
	{
		var parameters = string.Join(", ", ctor.Parameters.Select(p => SerializeParameter(p)));
		var methodName = ctor.Name.ToPascalCase().Replace(className, "");
		var externParams = string.Join(", ", ctor.Parameters.Select(p => $"{p.Modifier} {p.Name}".Trim()));

		if (ctor.HasErrorParam && ctor.Parameters.Any()) externParams += ", out var error";
		else if (ctor.HasErrorParam) externParams = "out var error";

		var externCall = $"{className}Externs.{ctor.ExternName}({externParams});";
		var output = new StringBuilder();
		output.AppendLine($"\tpublic static {SerializeType(ctor.ReturnValue.Type)} {methodName}({parameters})");
		output.AppendLine("\t{");

		if (ctor.HasErrorParam)
		{
			output.AppendLine($"\t\tvar externCallResult = {externCall}");
			if (ctor.HasErrorParam) output.AppendLine($"\t\t{CheckAndThrowException}");
			output.AppendLine("\t\treturn externCallResult;");
		}
		else
		{
			output.AppendLine($"\t\treturn {externCall}");
		}

		output.AppendLine("\t}");
		return output.ToString();
	}

	private const string CheckAndThrowException = "if (!error.IsInvalid) throw new Exception(error.Dereference().message);";

	private string SerializeMethod(ConvertedMethod m, string className, bool allowGenerics = true)
	{
		var methodName = m.Name.ToPascalCase().Replace(className, "");
		var externParams = string.Join(", ", m.Parameters.Select(p => $"{p.Modifier} {p.Name}".Trim()));

		if (m.HasErrorParam && m.Parameters.Any()) externParams += ", out var error";
		else if (m.HasErrorParam) externParams = "out var error";

		var externCall = $"{className}Externs.{m.ExternName}({externParams});";
		var returnType = SerializeType(m.ReturnValue.Type);
		var output = new StringBuilder();

		if (m.IsInstanceMethod && m.Parameters.First().ConvertedType.IsSafeHandle)
		{
			var instanceParam = m.Parameters.First();
			var otherSerializedParams = m.Parameters.Skip(1).Select(p => SerializeParameter(p)).ToList();

			if (returnType == "void" && allowGenerics)
			{
				var serializedInstanceParams = $"this T {instanceParam.Name.NormalizeName()}";
				var allSerializedParams = string.Join(", ", new List<string>() { serializedInstanceParams }.Concat(otherSerializedParams));
				output.AppendLine($"\tpublic static T {methodName}<T>({allSerializedParams}) where T : {className}");
				output.AppendLine("\t{");
				output.AppendLine($"\t\tif ({instanceParam.Name.NormalizeName()}.IsInvalid) throw new Exception(\"Invalid handle ({className})\");");
				output.AppendLine($"\t\t{externCall}");
				if (m.HasErrorParam) output.AppendLine($"\t\t{CheckAndThrowException}");
				output.AppendLine($"\t\treturn {instanceParam.Name.NormalizeName()};");
				output.AppendLine("\t}");
			}
			else if (returnType == "void")
			{
				var serializedInstanceParams = $"this {SerializeType(instanceParam.ConvertedType)} {instanceParam.Name.NormalizeName()}";
				var allSerializedParams = string.Join(", ", new List<string>() { serializedInstanceParams }.Concat(otherSerializedParams));
				output.AppendLine($"\tpublic static void {methodName}({allSerializedParams})");
				output.AppendLine("\t{");
				output.AppendLine($"\t\tif ({instanceParam.Name.NormalizeName()}.IsInvalid) throw new Exception(\"Invalid handle ({className})\");");
				output.AppendLine($"\t\t{externCall}");
				if (m.HasErrorParam) output.AppendLine($"\t\t{CheckAndThrowException}");
				output.AppendLine("\t}");
			}
			else
			{
				var serializedInstanceParams = $"this {SerializeType(instanceParam.ConvertedType)} {instanceParam.Name.NormalizeName()}";
				var allSerializedParams = string.Join(", ", new List<string>() { serializedInstanceParams }.Concat(otherSerializedParams));
				output.AppendLine($"\tpublic static {returnType} {methodName}({allSerializedParams})");
				output.AppendLine("\t{");
				output.AppendLine($"\t\tif ({instanceParam.Name.NormalizeName()}.IsInvalid) throw new Exception(\"Invalid handle ({className})\");");

				if (m.HasErrorParam)
				{
					output.AppendLine($"\t\tvar externCallResult = {externCall}");
					if (m.HasErrorParam) output.AppendLine($"\t\t{CheckAndThrowException}");
					output.AppendLine("\t\treturn externCallResult;");
				}
				else
				{
					output.AppendLine($"\t\treturn {externCall}");
				}

				output.AppendLine("\t}");
			}
		}
		else if (returnType == "void")
		{
			var parameters = string.Join(", ", m.Parameters.Select(p => SerializeParameter(p)));
			output.AppendLine($"\tpublic static void {methodName}({parameters})");
			output.AppendLine("\t{");
			output.AppendLine($"\t\t{externCall}");
			if (m.HasErrorParam) output.AppendLine($"\t\t{CheckAndThrowException}");
			output.AppendLine("\t}");
		}
		else
		{
			var parameters = string.Join(", ", m.Parameters.Select(p => SerializeParameter(p)));
			output.AppendLine($"\tpublic static {returnType} {methodName}({parameters})");
			output.AppendLine("\t{");

			if (m.HasErrorParam)
			{
				output.AppendLine($"\t\tvar externCallResult = {externCall}");
				if (m.HasErrorParam) output.AppendLine($"\t\t{CheckAndThrowException}");
				output.AppendLine("\t\treturn externCallResult;");
			}
			else
			{
				output.AppendLine($"\t\treturn {externCall}");
			}

			output.AppendLine("\t}");
		}
		return output.ToString();
	}

	private const string StringMarshallerAttribute = "[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NoNativeFreeStringMarshaller))]";
	private const string StringArrayMarshallerAttribute = "[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ReadNullTerminatedArrayMarshaller<NoNativeFreeStringMarshaller, string>))]";
	private const string SafeHandleMarshallerAttribute = "[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DelegateSafeHandleMarshaller<{safeHandleTypeName}>))]";
	private const string ConstructorSafeHandleMarshallerAttribute = "[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstructorSafeHandleMarshaller<{safeHandleTypeName}>))]";

	private bool IsGObjectHandle(string typeName)
	{
		if (typeName == "GObjectHandle") return true;
		if (string.IsNullOrEmpty(typeName)) return false;
		var impl = _allNamespaces.SelectMany(ns => ns.Classes).FirstOrDefault(i => i.Name == typeName);
		if (impl == null) return false;
		return IsGObjectHandle(impl.Parent);
	}

	private string SerializeExternMethod(ConvertedMethod m, bool doNotAddRef = false)
	{
		var output = new StringBuilder();
		output.AppendLine($"\t[DllImport({_currentNamespace.Name}Library.Name)]");
		if (m.ReturnValue.Type.CSharpTypeName == "string") output.AppendLine("\t" + StringMarshallerAttribute);
		if (m.ReturnValue.Type.CSharpTypeName == "string[]") output.AppendLine("\t" + StringArrayMarshallerAttribute);

		if (m.ReturnValue.Type.CSharpTypeName.EndsWith("Handle") && !m.ReturnValue.Type.IsBasicArray)
		{
			var safeHandleTypeName = SerializeType(m.ReturnValue.Type);
			if (m.ReturnValue.Type.IsInterface) safeHandleTypeName += "Impl";
			var attribute = doNotAddRef ? ConstructorSafeHandleMarshallerAttribute : SafeHandleMarshallerAttribute;
			output.AppendLine("\t" + attribute.Replace("{safeHandleTypeName}", safeHandleTypeName));
		}

		var parameters = string.Join(", ", m.Parameters.Select(p => SerializeParameter(p, true, m.IsInstanceMethod)));

		if (m.HasErrorParam && m.Parameters.Any()) parameters += ", out MentorLake.GLib.GErrorHandle error";
		else if (m.HasErrorParam) parameters = "out MentorLake.GLib.GErrorHandle error";

		output.AppendLine($"\tinternal static extern {SerializeType(m.ReturnValue.Type)} {m.ExternName}({parameters});");
		return output.ToString();
	}

	private string SerializeField(ConvertedField field)
	{
		var type = "";
		if (field.Callback != null) type = "IntPtr";
		else if (field.Type != null) type = field.Type.IsPointer ? "IntPtr" : field.Type.CSharpTypeName;
		else throw new Exception("Unknown field type: " + field.Name);

		var output = new StringBuilder();
		if (type != "IntPtr" && field.Type is { IsBasicArray: true }) output.Append("[MarshalAs(UnmanagedType.ByValArray)] ");
		output.Append($"public {type} {field.Name};");
		return output.ToString();
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
		output.AppendLine();
		output.AppendLine($"\tpublic static {unionName} Dereference(this {unionName}Handle x) => System.Runtime.InteropServices.Marshal.PtrToStructure<{unionName}>(x.DangerousGetHandle());");
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
		var isInt = field.Members.All(kv => kv.Value >= int.MinValue) && field.Members.All(kv => kv.Value <= int.MaxValue);
		var isUInt = field.Members.All(kv => kv.Value >= 0) && field.Members.All(kv => kv.Value <= uint.MaxValue);
		var enumType = isUInt ? "uint" : isInt ? "int" : "long";
		output.AppendLine($"public enum {field.Name} : {enumType}");
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
		output.AppendLine("\tpublic bool IsInvalid { get; }");
		output.AppendLine("\tpublic bool IsClosed { get; }");
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

	public void SerializeNamespaces(List<ConvertedNamespace> namespaces, string outputBaseDirectory)
	{
		_allNamespaces = namespaces;

		foreach (var lib in namespaces)
		{
			Console.WriteLine($"Writing {lib.Name}...");
			WriteAllFiles(outputBaseDirectory, lib);
		}
	}

	public void WriteAllFiles(string outputBaseDirectory, ConvertedNamespace convertedNamespace)
	{
		_currentNamespace = convertedNamespace;

		var outputDir = Path.Join(outputBaseDirectory, _currentNamespace.Name);
		if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
		Directory.CreateDirectory(outputDir);

		var header = $"namespace MentorLake.{_currentNamespace.Name};";

		var libraryNameFile = $"{header}\r\n\r\npublic static class {_currentNamespace.Name}Library {{ public const string Name = \"{_currentNamespace.SharedLibrary}\"; }}";
		File.WriteAllText(Path.Join(outputDir, $"{_currentNamespace.Name}Library.cs"), libraryNameFile);

		foreach (var cb in convertedNamespace.Callbacks)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeCallback(cb));
			File.WriteAllText(Path.Join(outputDir, cb.Name + ".cs"), output.ToString());
		}

		foreach (var alias in convertedNamespace.Aliases)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeAlias(alias));
			File.WriteAllText(Path.Join(outputDir, alias.Name + ".cs"), output.ToString());
		}

		foreach (var record in convertedNamespace.Records)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeUnion(record));
			File.WriteAllText(Path.Join(outputDir, record.Name + ".cs"), output.ToString());
		}

		foreach (var union in convertedNamespace.Unions)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeUnion(union));
			File.WriteAllText(Path.Join(outputDir, union.Name + ".cs"), output.ToString());
		}

		foreach (var s in convertedNamespace.Bitfields)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeBitfield(s));
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var s in convertedNamespace.Enumerations)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeEnumeration(s));
			File.WriteAllText(Path.Join(outputDir, s.Name + ".cs"), output.ToString());
		}

		foreach (var i in convertedNamespace.Interfaces)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeInterface(i));
			File.WriteAllText(Path.Join(outputDir, i.Name + ".cs"), output.ToString());
		}

		foreach (var c in convertedNamespace.Classes)
		{
			var output = new StringBuilder();
			output.AppendLine(header).AppendLine().Append(SerializeClass(c));
			File.WriteAllText(Path.Join(outputDir, $"{c.Name}.cs"), output.ToString());
		}

		var globalFunctionsOutput = new StringBuilder();
		globalFunctionsOutput.AppendLine(header);
		globalFunctionsOutput.AppendLine();
		globalFunctionsOutput.AppendLine($"public class {_currentNamespace.Name}GlobalFunctions");
		globalFunctionsOutput.AppendLine("{");
		foreach (var f in convertedNamespace.Functions) globalFunctionsOutput.AppendLine(SerializeMethod(f, _currentNamespace.Name + "GlobalFunctions"));
		globalFunctionsOutput.AppendLine("}");
		globalFunctionsOutput.AppendLine();
		globalFunctionsOutput.AppendLine($"internal class {_currentNamespace.Name}GlobalFunctionsExterns");
		globalFunctionsOutput.AppendLine("{");
		foreach (var f in convertedNamespace.Functions) globalFunctionsOutput.AppendLine(SerializeExternMethod(f, f.TransferOwnership == ReturnValueTransferOwnership.Full && IsGObjectHandle(f.ReturnValue.Type.CSharpTypeName)));
		globalFunctionsOutput.AppendLine("}");
		File.WriteAllText(Path.Join(outputDir, $"{_currentNamespace.Name}GlobalFunctions.cs"), globalFunctionsOutput.ToString());

		var constants = new StringBuilder();
		constants.AppendLine(header);
		constants.AppendLine();
		constants.AppendLine($"public static class {_currentNamespace.Name}Constants");
		constants.AppendLine("{");

		foreach (var c in convertedNamespace.Constants)
		{
			if (c.Type.CSharpTypeName == "string")
			{
				constants.AppendLine($"\tpublic static {SerializeType(c.Type)} {c.Name.NormalizeName()} = \"{c.Value}\";");
			}
			else
			{
				constants.AppendLine($"\tpublic static {SerializeType(c.Type)} {c.Name.NormalizeName()} = {c.Value};");
			}
		}

		constants.AppendLine("}");
		File.WriteAllText(Path.Join(outputDir, $"{_currentNamespace.Name}Constants.cs"), constants.ToString());
	}
}
