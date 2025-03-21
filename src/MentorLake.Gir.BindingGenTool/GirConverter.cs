using System.Text.RegularExpressions;
using MentorLake.Gir.Core;
using MentorLake.Gir.GLib;
using Array = MentorLake.Gir.Core.Array;
using Type = MentorLake.Gir.Core.Type;

namespace BindingTransform.Serialization.Gir;

public class ConvertedType
{
	public string CSharpTypeName { get; set; }
	public string CType { get; set; }
	public bool IsCTypeSpecified { get => !string.IsNullOrEmpty(CType); }
	public string Namespace { get; set; }
	public string Name { get; set; }
	public bool FoundCTypeMatch { get; set; }
	public bool IsBasicArray { get; set; }
	public bool IsPointer { get; set; }
	public bool IsPointerReadOnly { get; set; }
	public bool IsDataReadOnly { get; set; }
	public bool IsBuiltInType { get; set; }
	public bool IsInterface { get; set; }
	public bool IsSafeHandle { get; set; }
	public int ArraySizeIndex { get; set; }
	public int ArrayDepth { get; set; }
}

public class ConvertedParameter()
{
	public ConvertedType ConvertedType { get; set; }
	public string Modifier { get; set; } = "";
	public string Name { get; set; }
}

public class ConvertedCallback
{
	public List<ConvertedParameter> Parameters { get; set; }
	public ConvertedReturnValue ReturnValue { get; set; }
	public string Name { get; set; }
}

public class ConvertedReturnValue
{
	public ConvertedType Type { get; set; }
	public string Docs { get; set; }
}

public class ConvertedAlias
{
	public ConvertedType WrappedType { get; init; }
	public string Name { get; set; }
}

public class ConvertedMethod
{
	public string Name { get; set; }
	public ConvertedReturnValue ReturnValue { get; set; }
	public List<ConvertedParameter> Parameters { get; set; }
	public ReturnValueTransferOwnership TransferOwnership { get; set; }
	public bool IsInstanceMethod { get; set; }
	public string ExternName { get; set; }
}

public class ConvertedField
{
	public string Name { get; set; }
	public ConvertedType Type { get; set; }
	public ConvertedCallback Callback { get; set; }
}

public class ConvertedProperty
{
	public string Name { get; set; }
	public ConvertedType Type { get; set; }
	public bool InitializeOnly { get; set; }
	public bool IsReadOnly { get; set; }
	public string GetFunc { get; set; }
	public string SetFunc { get; set; }
}

public class ConvertedUnion
{
	public string Name { get; set; }
	public List<ConvertedMethod> Constructors { get; set; }
	public List<ConvertedMethod> Methods { get; set; }
	public List<ConvertedMethod> Functions { get; set; }
	public List<ConvertedField> Fields { get; set; }
	public List<ConvertedUnion> Records { get; set; }
}

public class ConvertedBitField
{
	public string Name { get; set; }
	public List<KeyValuePair<string, long>> Members { get; set; }
}

public class ConvertedEnumeration
{
	public string Name { get; set; }
	public List<KeyValuePair<string, string>> Members { get; set; }
}

public class ConvertedSignal : ConvertedCallback
{

}

public class ConvertedConstant
{
	public string Name { get; set; }
	public string Value { get; set; }
	public ConvertedType Type { get; set; }
}

public class ConvertedInterface
{
	public string Name { get; set; }
	public List<string> Implements { get; set; }
	public List<ConvertedMethod> Functions { get; set; }
	public List<ConvertedMethod> Methods { get; set; }
	public List<ConvertedMethod> VirtualMethods { get; set; }
	public List<ConvertedField> Fields { get; set; }
	public List<ConvertedProperty> Properties { get; set; }
	public List<ConvertedMethod> Constructors { get; set; }
	public List<ConvertedSignal> Signals { get; set; }
	public List<ConvertedCallback> Callbacks { get; set; }
	public List<ConvertedConstant> Constants { get; set; }
}

public class ConvertedClass : ConvertedInterface
{
	public string Parent { get; set; }
}

public class GirConverter
{
	private readonly Namespace _currentNamespace;
	private readonly IEnumerable<Interface> _allInterfaces;
	private readonly List<Namespace> _namespaces;

	public GirConverter(Namespace currentNamespace, List<Repository> repositories)
	{
		_currentNamespace = currentNamespace;
		_namespaces = repositories.Select(r => r.Namespace.First()).ToList();
		_allInterfaces = _namespaces.Where(ns => ns.Interface != null).SelectMany(ns => ns.Interface).ToList();
	}

	private static readonly string[] s_builtInTypes = {
		"bool",
		"byte",
		"sbyte",
		"char",
		"decimal",
		"double",
		"float",
		"int",
		"uint",
		"long",
		"ulong",
		"short",
		"ushort",
		"string",
		"IntPtr",
		"UIntPtr",
		"nint",
		"nuint",
		"void"
	};

	private static readonly List<(Regex r, string result)> s_patterns =
	[
		(new("void"), "void"),
		(new("none"), "void"),
		(new("utf8"), "string"),
		(new(@"^g?int64"), "long"),
		(new(@"^g?uint64"), "ulong"),
		(new(@"^g?int32"), "int"),
		(new(@"^g?uint32"), "uint"),
		(new(@"^g?int16"), "short"),
		(new(@"^g?uint16"), "ushort"),
		(new(@"^g?u?int8"), "byte"),
		(new(@"^unsigned int"), "uint"),
		(new(@"^unsigned long"), "ulong"),
		(new(@"^unsigned short"), "ushort"),
		(new(@"^g?int"), "int"),
		(new(@"^g?uint"), "uint"),
		(new(@"^g?double"), "double"),
		(new(@"^g?long"), "long"),
		(new(@"^g?ulong"), "ulong"),
		(new(@"^g?short"), "short"),
		(new(@"^g?ushort"), "ushort"),
		(new(@"^g?float"), "float"),
		(new(@"^g?(u|uni)?char"), "char"),
		(new(@"^g?boolean"), "bool"),
		(new(@"^?g?intptr"), "nint"),
		(new(@"^?g?uintptr"), "nuint"),
		(new(@"^grefcount"), "int"),
		(new(@"^goffset"), "int"),
		(new(@"^(gpointer|gconstpointer)"), "IntPtr"),
		(new(@"^gss?ize"), "UIntPtr"),
		(new(@"^size_t"), "UIntPtr"),
		(new(@"^time_t"), "long"),
		(new(@"^pid_t"), "int"),
		(new(@"^uid_t"), "int"),
		(new(@"^tm\*"), "IntPtr"),
		(new(@"^XID"), "ulong"),
		(new(@"^gatomicrefcount"), "int"),
		(new(@"^va_list"), "IntPtr"),
		(new(@"^filename"), "string"),
		(new(@"^passwd"), "IntPtr"),
		(new(@"^utimbuf"), "IntPtr"),
		(new(@"^FILE"), "IntPtr")
	];

	private string MapToBuiltInType(string typeName)
	{
		return s_patterns.FirstOrDefault(p => p.r.IsMatch(typeName)).result;
	}

	private string TrimPointerSymbol(string s, string errorMessage)
	{
		if (!s.EndsWith("*"))
		{
			Console.WriteLine(errorMessage);
			return s;
		}

		return s.Substring(0, s.Length - 1);
	}

	private void ConvertCTypeToCSharpType(ConvertedType convertedType, bool isOutParam)
	{
		if (convertedType.Name == "utf8")
		{
			convertedType.CSharpTypeName = "string";
			convertedType.IsBuiltInType = true;
			convertedType.FoundCTypeMatch = true;
			return;
		}

		if (convertedType.Name == "gpointer" && convertedType.CType == "void")
		{
			convertedType.CSharpTypeName = "IntPtr";
			convertedType.IsBuiltInType = true;
			convertedType.FoundCTypeMatch = true;
			return;
		}

		var result = convertedType.CType is "gpointer" or "gconstpointer" ?  convertedType.Name + "*" : convertedType.CType;
		result = result.Replace("volatile ", "");
		convertedType.IsDataReadOnly = result.StartsWith("const");
		convertedType.IsPointerReadOnly = result.EndsWith("const");
		if (convertedType.IsDataReadOnly) result = result.Replace("const ", "");
		if (convertedType.IsPointerReadOnly) result = result.Replace(" const", "");
		if (isOutParam) result = TrimPointerSymbol(result, "Out param didn't have a *");

		if (result.EndsWith("*"))
		{
			convertedType.IsPointer = true;
			result = TrimPointerSymbol(result, "Should never fail");
		}

		result = MapToBuiltInType(result) ?? result;
		if (convertedType.IsPointer && result == "void") result = "IntPtr";
		if (isOutParam && result == "void") result = "IntPtr";

		if (result.Contains("*"))
		{
			Console.WriteLine($"WARNING: Couldn't convert C type [{convertedType.CType}].  Algorithm produced [{result}].  Using IntPtr instead.");
			result = "IntPtr";
		}

		var isBuiltInType = s_builtInTypes.Contains(result);
		var cTypeMatch = FindCType(result, convertedType.Namespace);
		convertedType.FoundCTypeMatch = !string.IsNullOrEmpty(cTypeMatch.match) || isBuiltInType;
		convertedType.Namespace = convertedType.FoundCTypeMatch ? cTypeMatch.ns : convertedType.Namespace;
		convertedType.IsBuiltInType = isBuiltInType;
		convertedType.CSharpTypeName = result;
	}

	private ConvertedType FindTypeToConvert(object anyType)
	{
		var array = anyType as Array;
		var type = anyType as Type;
		var arrayType = array;
		var arrayDepth = 0;

		while (array != null)
		{
			arrayDepth++;
			type = array.AnyType as Type;
			array = array.AnyType as Array;
			if (array != null) arrayType = array;
		}

		var typeName = type.Name;
		var cType = type.TypeProperty;
		var isBasicArray = false;
		var arraySizeIndex = -1;

		if (arrayType != null)
		{
			if (!string.IsNullOrEmpty(arrayType.Name))
			{
				typeName = arrayType.Name;
				cType = arrayType.Type;
			}
			else
			{
				typeName = type.Name;
				cType = type.TypeProperty;
				arraySizeIndex = !string.IsNullOrEmpty(arrayType.Length) ? int.Parse(arrayType.Length) : -1;
				isBasicArray = true;
			}
		}

		var typeNameParts = !string.IsNullOrEmpty(typeName) && typeName.Contains(".") ? typeName.Split(".") : !string.IsNullOrEmpty(typeName) ? [typeName] : [];
		var parsedTypeName = typeNameParts.Length == 0 ? "" : typeNameParts.Length == 1 ? typeNameParts[0] : typeNameParts[1];
		var parsedNamespace = typeNameParts.Length <= 1 ? _currentNamespace.Name : typeNameParts[0];

		return new ConvertedType()
		{
			Namespace = parsedNamespace,
			Name = parsedTypeName,
			CType = cType,
			IsBasicArray = isBasicArray,
			ArrayDepth = arrayDepth,
			ArraySizeIndex = arraySizeIndex,
		};
	}

	private ConvertedType ConvertTypeRef(object anyType, bool isOutParam = false)
	{
		var result = FindTypeToConvert(anyType);

		if (result.IsCTypeSpecified)
		{
			ConvertCTypeToCSharpType(result, isOutParam);
			//if (!result.FoundCTypeMatch) Console.WriteLine($"WARNING: No type matched for CType: [{result.CType}], Searched Using: [{result.CSharpTypeName}].  Searching again using [{result.Name}]");
		}

		if (!result.IsCTypeSpecified || !result.FoundCTypeMatch)
		{
			var nameMatch = ConvertNameToCType(result.Name, result.Namespace);
			result.CSharpTypeName = nameMatch.match ?? MapToBuiltInType(result.Name) ?? result.Name;
			result.IsPointer = IsObjectType(result.CSharpTypeName, nameMatch.ns);
			result.IsBuiltInType = s_builtInTypes.Contains(result.CSharpTypeName);
		}

		if (!result.IsBuiltInType && result.IsPointer && !IsCallback(result.CSharpTypeName, result.Namespace))
		{
			result.IsInterface = _allInterfaces.Any(i => i.Name == result.CSharpTypeName || i.Type == result.CSharpTypeName || i.Type == result.CSharpTypeName);
			result.IsSafeHandle = true;
			result.CSharpTypeName += "Handle";
		}

		if (result.IsBasicArray)
		{
			result.CSharpTypeName += string.Concat(Enumerable.Repeat("[]", result.ArrayDepth));
		}

		return result;
	}

	private string ConvertParameterName(BaseParam m)
	{
		if (m.Varargs != null) return "@__arglist";
		return m.Name.NormalizeName();
	}

	private ConvertedParameter ConvertParameter(BaseParam param)
	{
		return new ConvertedParameter()
		{
			Name = ConvertParameterName(param),
			ConvertedType = ConvertParameterType(param),
			Modifier = !param.DirectionSpecified ? "" : param.Direction switch
			{
				BaseParamDirection.Out => "out",
				BaseParamDirection.Inout => "ref",
				_ => ""
			}
		};
	}

	private ConvertedType ConvertParameterType(BaseParam param)
	{
		if (param.Varargs != null) return new() { CSharpTypeName = "IntPtr", IsBuiltInType = true };
		var isOutParameter = param.DirectionSpecified && param.Direction == BaseParamDirection.Out;
		var isInOutParameter = param.DirectionSpecified && param.Direction == BaseParamDirection.Inout;
		return ConvertTypeRef(param.AnyType, isOutParameter || isInOutParameter);
	}

	private ConvertedReturnValue ConvertReturnValue(ReturnValue returnValue)
	{
		return new ConvertedReturnValue()
		{
			Type = ConvertTypeRef(returnValue.AnyType)
		};
	}

	public ConvertedCallback ConvertCallback(Callback cb)
	{
		return new ConvertedCallback()
		{
			Name = cb.Type,
			Parameters = cb.Parameters != null ? cb.Parameters.Parameter.Select(ConvertParameter).ToList() : new(),
			ReturnValue = ConvertReturnValue(cb.ReturnValue)
		};
	}

	public ConvertedAlias ConvertAlias(Alias alias)
	{
		return new ConvertedAlias()
		{
			Name = alias.Type,
			WrappedType = ConvertTypeRef(alias.AnyType)
		};
	}

	private ConvertedMethod ConvertConstructor(Constructor ctor, string className)
	{
		return new ConvertedMethod()
		{
			Name = ctor.Name,
			ExternName = ctor.Identifier,
			ReturnValue = new() { Type = new() { CSharpTypeName = className + "Handle", Namespace = _currentNamespace.Name } },
			Parameters = ctor.Parameters != null ? ctor.Parameters.Parameter.Select(ConvertParameter).ToList() : new(),
		};
	}

	public ConvertedMethod ConvertFunction(BaseFunction m)
	{
		var parameters = new List<BaseParam>();

		if (m.Parameters != null)
		{
			parameters = m.Parameters.InstanceParameterSpecified ? m.Parameters.InstanceParameter : new();
			parameters = parameters.Concat(m.Parameters.ParameterSpecified ? m.Parameters.Parameter : new()).ToList();
		}

		return new ConvertedMethod()
		{
			Name = !string.IsNullOrEmpty(m.Name) ? m.Name.ToPascalCase() : m.Identifier.ToPascalCase(),
			ExternName = m.Identifier,
			ReturnValue = ConvertReturnValue(m.ReturnValue),
			Parameters = parameters.Select(ConvertParameter).ToList(),
			TransferOwnership = m.ReturnValue.TransferOwnership,
			IsInstanceMethod = m.Parameters?.InstanceParameterSpecified ?? false
		};
	}

	private ConvertedField ConvertField(Field field)
	{
		return new ConvertedField()
		{
			Name = field.Name.NormalizeName(),
			Callback = field.Callback == null ? null : ConvertCallback(field.Callback),
			Type = field.AnyType == null ? null : ConvertTypeRef(field.AnyType)
		};
	}

	private ConvertedProperty ConvertProperty(Property property)
	{
		return new ConvertedProperty()
		{
			Name = property.Name.NormalizeName(),
			InitializeOnly = property.ConstructOnlySpecified && property.Construct == PropertyConstruct.Item1,
			IsReadOnly = property.WritableSpecified && property.Writable != PropertyWritable.Item1,
			Type = ConvertTypeRef(property.AnyType),
			GetFunc = property.Getter,
			SetFunc = property.Setter
		};
	}

	private ConvertedSignal ConvertSignal(Signal signal, string className)
	{
		var parameters = (signal.Parameters?.Parameter ?? new()).Select(ConvertParameter).ToList();
		parameters.Insert(0, new ConvertedParameter() { Name = "self", ConvertedType = new() { CSharpTypeName = className, Namespace = _currentNamespace.Name, IsPointer = true, IsSafeHandle = true }, Modifier = "" });
		parameters.Add(new ConvertedParameter() { Name = "user_data", ConvertedType = new() { CSharpTypeName = "IntPtr", IsBuiltInType = true }, Modifier = "" });

		return new ConvertedSignal()
		{
			Name = signal.Name,
			Parameters = parameters,
			ReturnValue = ConvertReturnValue(signal.ReturnValue)
		};
	}

	private ConvertedConstant ConvertConstant(Constant constant)
	{
		return new ConvertedConstant()
		{
			Name = constant.Identifier,
			Value = constant.Value,
			Type = ConvertTypeRef(constant.AnyType)
		};
	}

	public ConvertedUnion ConvertRecord(Record record)
	{
		var publicFields = record.Field.Where(f => !f.PrivateSpecified || f.Private == FieldPrivate.Item0).ToList();

		return new ConvertedUnion()
		{
			Name = record.Type,
			Constructors = record.Constructor.Select(c => ConvertConstructor(c, record.Type)).ToList(),
			Methods = ConvertList(record.Method, ConvertFunction),
			Functions = ConvertList(record.Function, ConvertFunction),
			Fields = record.OpaqueSpecified && record.Opaque == RecordOpaque.Item1 ? new() : ConvertList(publicFields, ConvertField)
		};
	}

	// Fields and records need to put in the order they're specified.  Currently can't do this with the way it's deserializing.
	public ConvertedUnion ConvertUnion(Union union)
	{
		return new ConvertedUnion()
		{
			Name = union.Type ?? union.Name,
			Constructors = ConvertList(union.Constructor, c => ConvertConstructor(c, union.Type)),
			Methods = ConvertList(union.Method, ConvertFunction),
			Functions = ConvertList(union.Function, ConvertFunction),
			Fields = ConvertList(union.Field, ConvertField),
			Records = ConvertList(union.Record, ConvertRecord)
		};
	}

	public ConvertedBitField ConvertBitField(Bitfield bitfield)
	{
		return new ConvertedBitField()
		{
			Name = bitfield.Type,
			Members = bitfield.Member.Select(m => KeyValuePair.Create(m.Identifier, long.Parse(m.Value))).ToList()
		};
	}

	public ConvertedEnumeration ConvertEnumeration(Enumeration enumeration)
	{
		return new ConvertedEnumeration()
		{
			Name = enumeration.Type,
			Members = enumeration.Member.Select(m => KeyValuePair.Create(m.Identifier, m.Value)).ToList()
		};
	}

	public ConvertedInterface ConvertInterface(Interface i)
	{
		return new ConvertedInterface()
		{
			Name = i.Type + "Handle",
			Implements = i.Implements?.Select(x => x.Name).ToList(),
			Constructors = ConvertList([i.Constructor], c => ConvertConstructor(c, i.Type)),
			Functions = ConvertList(i.Function, ConvertFunction),
			Methods = ConvertList(i.Method, ConvertFunction),
			VirtualMethods = ConvertList(i.VirtualMethod, ConvertFunction),
			Properties = ConvertList(i.Property, ConvertProperty),
			Fields = ConvertList(i.Field, ConvertField),
			Signals = ConvertList(i.Signal, s => ConvertSignal(s, i.Type + "Handle")),
			Callbacks = ConvertList(i.Callback, ConvertCallback),
			Constants = ConvertList(i.Constant, ConvertConstant)
		};
	}

	private List<TOut> ConvertList<TIn, TOut>(List<TIn> list, Func<TIn, TOut> conversionFunc) where TIn : IInfoAttrs
	{
		if (list == null) return new List<TOut>();
		return list.Where(e => e != null).Select(conversionFunc).ToList();
	}

	private (string ns, string match) FindCType(string cTypeName, string namespaceNameToSearch)
	{
		if (string.IsNullOrEmpty(cTypeName)) return default;
		var namespacesToSearch = new[] { _namespaces.First(ns => ns.Name == namespaceNameToSearch), _namespaces.First(ns => ns.Name == "GObject") };

		foreach (var ns in namespacesToSearch)
		{
			var match = ns?.Interface.FirstOrDefault(i => i.Type == cTypeName)?.Type
				?? ns?.Class.FirstOrDefault(i => i.Type == cTypeName)?.Type
				?? ns?.Union.FirstOrDefault(i => i.Type == cTypeName)?.Type
				?? ns?.Alias.FirstOrDefault(i => i.Type == cTypeName)?.Type
				?? ns?.Enumeration.FirstOrDefault(i => i.Type == cTypeName)?.Type
				?? ns?.Callback.FirstOrDefault(i => i.Type == cTypeName)?.Type
				?? ns?.Bitfield.FirstOrDefault(i => i.Type == cTypeName)?.Type
				?? ns?.Record.FirstOrDefault(i => i.Type == cTypeName)?.Type;

			if (match != null) return (ns.Name, match);
		}

		return default;
	}

	private bool IsCallback(string cTypeName, string namespaceNameToSearch)
	{
		var ns = _namespaces.FirstOrDefault(ns => ns.Name == namespaceNameToSearch);
		if (ns == null) return false;
		return ns.Callback.Any(i => i.Type == cTypeName);
	}

	private bool IsObjectType(string cTypeName, string namespaceNameToSearch)
	{
		var ns = _namespaces.FirstOrDefault(ns => ns.Name == namespaceNameToSearch);
		if (ns == null) return false;
		return ns.Interface.Any(i => i.Type == cTypeName)
			|| ns.Class.Any(i => i.Type == cTypeName)
			|| ns.Union.Any(i => i.Type == cTypeName)
			|| ns.Alias.Any(i => i.Type == cTypeName)
			|| ns.Record.Any(i => i.Type == cTypeName);
	}

	private (string ns, string match) ConvertNameToCType(string name, string namespaceNameToSearch)
	{
		if (string.IsNullOrEmpty(name)) return default;

		var namespacesToSearch = new[] { _namespaces.First(ns => ns.Name == namespaceNameToSearch), _namespaces.First(ns => ns.Name == "GObject") };

		foreach (var ns in namespacesToSearch)
		{
			var match = ns?.Interface.FirstOrDefault(i => i.Name == name)?.Type
				?? ns?.Class.FirstOrDefault(i => i.Name == name)?.Type
				?? ns?.Union.FirstOrDefault(i => i.Name == name)?.Type
				?? ns?.Alias.FirstOrDefault(i => i.Name == name)?.Type
				?? ns?.Enumeration.FirstOrDefault(i => i.Name == name)?.Type
				?? ns?.Callback.FirstOrDefault(i => i.Name == name)?.Type
				?? ns?.Bitfield.FirstOrDefault(i => i.Name == name)?.Type
				?? ns?.Record.FirstOrDefault(i => i.Name == name)?.Type;

			if (match != null) return (ns.Name, match);
		}

		return default;
	}

	public ConvertedClass ConvertClass(Class c)
	{
		var className = !string.IsNullOrEmpty(c.Type) ? c.Type : c.TypeName;

		var interfaceNames = c.Implements == null ? new() : c.Implements.Select(i =>
		{
			var iParts = i.Name.Split(".");
			var namespaceToSearch = iParts.Length > 1 ? iParts[0] : _currentNamespace.Name;
			var match = ConvertNameToCType(iParts.Last(), namespaceToSearch).match;
			if (match != null) return match + "Handle";
			Console.WriteLine("FAILED TO FIND INTERFACE: " + i.Name);
			return null;
		}).Where(i => i != null).ToList();

		var parentClassName = "";

		if (!string.IsNullOrEmpty(c.Parent))
		{
			var parentParts = c.Parent.Split(".");
			var namespaceToSearch = parentParts.Length > 1 ? parentParts[0] : _currentNamespace.Name;
			var match = ConvertNameToCType(parentParts.Last(), namespaceToSearch).match;
			if (match != null) parentClassName = match + "Handle";
			else Console.WriteLine($"FAILED TO FIND PARENT CLASS [{className}] [{c.Parent}]" );
		}

		if (string.IsNullOrEmpty(parentClassName))
		{
			parentClassName = "BaseSafeHandle";
		}

		return new ConvertedClass()
		{
			Name = className + "Handle",
			Implements = interfaceNames,
			Functions = ConvertList(c.Function, ConvertFunction),
			Methods = ConvertList(c.Method, ConvertFunction),
			VirtualMethods = ConvertList(c.VirtualMethod, ConvertFunction),
			Properties = ConvertList(c.Property, ConvertProperty),
			Fields = ConvertList(c.Field, ConvertField),
			Constructors = ConvertList(c.Constructor, m => ConvertConstructor(m, c.Type ?? c.TypeName)),
			Signals = ConvertList(c.Signal, s => ConvertSignal(s, className + "Handle")),
			Callbacks = ConvertList(c.Callback, ConvertCallback),
			Constants = ConvertList(c.Constant, ConvertConstant),
			Parent = parentClassName
		};
	}
}
