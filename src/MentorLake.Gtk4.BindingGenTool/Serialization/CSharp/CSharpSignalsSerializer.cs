using System.Text;

namespace BindingTransform.Serialization.CSharp;

public class CSharpSignalsSerializer
{
	public static string Serialize(ClassDeclaration c, LibraryDeclaration libraryDeclaration, List<LibraryDeclaration> libraries)
	{
		var output = new StringBuilder();
		output.AppendLine($"public static class {c.Name}SignalExtensions");
		output.AppendLine("{");

		foreach (var signal in c.Signals)
		{
			var handlerReturn = signal.ReturnType.ToCString() != "void" ? "signalStruct.ReturnValue" : "";
			var outParameterDefaultAssignments = string.Join("\n\t\t\t", signal.Parameters.Where(p => p.IsOutParameter()).Select(p => $"{p.Name} = default;"));

			var method = @$"
	public static IObservable<{c.Name}SignalStructs.{signal.Name.ToPascalCase()}Signal> Signal_{signal.Name.ToPascalCase()}(this {c.Name}Handle instance)
	{{
		return Observable.Create((IObserver<{c.Name}SignalStructs.{signal.Name.ToPascalCase()}Signal> obs) =>
		{{
			{c.Name}SignalDelegates.{signal.Name.NormalizeName()} handler = ({string.Join(", ", signal.Parameters.Select(p => $"{p.ToCSharpTypeWithModifiers(libraries)} {p.ToCSharpParameterName()}"))}) =>
			{{
				{outParameterDefaultAssignments}

				var signalStruct = new {c.Name}SignalStructs.{signal.Name.ToPascalCase()}Signal()
				{{
					{string.Join(", ", signal.Parameters.Select(p => $"{p.ToCSharpParameterName().ToPascalCase()} = {p.ToCSharpParameterName()}"))}
				}};

				obs.OnNext(signalStruct);
				return {handlerReturn};
			}};

			var handlerId = GObjectExterns.g_signal_connect_data(instance, ""{signal.Name}"", Marshal.GetFunctionPointerForDelegate(handler), IntPtr.Zero, null, GConnectFlags.G_CONNECT_AFTER);

			return Disposable.Create(() =>
			{{
				instance.GSignalHandlerDisconnect(handlerId);
				obs.OnCompleted();
			}});
		}});
	}}";
			output.AppendLine(method);
		}

		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"public static class {c.Name}SignalStructs");
		output.AppendLine("{");
		foreach (var s in c.Signals)
		{
			output.AppendLine();
			output.AppendLine($"public struct {s.Name.ToPascalCase()}Signal");
			output.AppendLine("{");

			foreach (var p in s.Parameters)
			{
				output.AppendLine($"\tpublic {p.ToCSharpTypeWithoutModifiers(libraries)} {p.ToCSharpParameterName().ToPascalCase()};");
			}

			if (s.ReturnType.ToCString() != "void")
			{
				output.AppendLine($"\tpublic {s.ToCSharpReturnType()} ReturnValue;");
			}

			output.AppendLine("}");
		}

		output.AppendLine("}");

		output.AppendLine();
		output.AppendLine($"public static class {c.Name}SignalDelegates");
		output.AppendLine("{");

		foreach (var s in c.Signals)
		{
			output.AppendLine();
			output.AppendLine(CSharpDelegateSerializer.Serialize(s, libraries));
		}

		output.AppendLine("}");
		return output.ToString();
	}
}
