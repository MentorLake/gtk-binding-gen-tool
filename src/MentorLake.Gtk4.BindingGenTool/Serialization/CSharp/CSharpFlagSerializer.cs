using System.Text;

namespace BindingTransform.Serialization.CSharp;

public static class CSharpFlagSerializer
{
	public static string Serialize(EnumDeclaration s)
	{
		var output = new StringBuilder();
		output.AppendLine("[Flags]");
		output.AppendLine($"public enum {s.Name}");
		output.AppendLine("{");

		for (var i = 0; i < s.Values.Count; i++)
		{
			var bitVal = i == 0 || i == 1 ? i : 1 << i - 1;
			output.Append("\t" + s.Values[i] + " = " + bitVal);
			if (i < s.Values.Count - 1) output.Append(",");
			output.AppendLine();
		}

		output.AppendLine("}");
		return output.ToString();
	}
}
