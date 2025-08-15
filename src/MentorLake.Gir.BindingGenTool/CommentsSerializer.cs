using System.Text;
using System.Text.RegularExpressions;
using BindingTransform.Serialization.Gir;

namespace BindingTransform;

public static class CommentsSerializer
{
	public static string SerializeSummaryComments(string[] comments)
	{
		if (comments == null || !comments.Any()) return "";

		var output = new StringBuilder();
		output.AppendLine("/// <summary>");
		output.Append(ReadParagraphs(comments));
		output.AppendLine("/// </summary>");
		return output.ToString();
	}

	private static List<string> Normalize(string[] comments)
	{
		return comments
			.SelectMany(c => c.Split('\n'))
			.Select(c => Regex.Replace(c, "\\[`([^\\]]*)`(?: key)?\\]\\(([^\\)]*)\\)", "<see href=\"$2\">$1</see>"))
			.Select(c => c.Replace("<", "&lt;"))
			.Select(c => c.Replace(">", "&gt;"))
			.Select(c => c.Replace("&", "&amp;"))
			.Select(c => c.Replace("\"", "&quot;"))
			.Select(c => c.Replace("'", "&apos;"))
			.ToList();
	}
	private static string ReadParagraphs(string[] comments)
	{
		var cleanedLines = Normalize(comments);
		var output = new StringBuilder();
		var inParagraph = false;

		for (int i = 0; i < cleanedLines.Count; i++)
		{
			string s = cleanedLines[i];
			if (s.Contains("|["))
			{
				s = cleanedLines[++i];
				output.AppendLine("/// <code>");

				while (i < cleanedLines.Count && !s.Contains("]|"))
				{
					output.AppendLine("/// " + s);
					s = cleanedLines[i++];
				}

				output.AppendLine("/// </code>");
			}
			else if (inParagraph && string.IsNullOrEmpty(s))
			{
				output.AppendLine("/// </para>");
				inParagraph = false;
			}
			else if (!inParagraph && !string.IsNullOrEmpty(s))
			{
				output.AppendLine("/// <para>");
				output.AppendLine("/// " + s);
				inParagraph = true;
			}
			else
			{
				output.AppendLine("/// " + s);
			}
		}

		if (inParagraph)
		{
			output.AppendLine("/// </para>");
		}

		return output.ToString();
	}

	public static string SerializeCallbackComments(ConvertedCallback m)
	{
		if (m.Comments == null) return "";

		var output = new StringBuilder();
		output.AppendLine(SerializeSummaryComments(m.Comments));

		foreach (var p in m.Parameters)
		{
			output.AppendLine($"/// <param name=\"{p.Name}\">");
			foreach (var s in Normalize(p.Comments)) output.AppendLine($"/// {s}");
			output.AppendLine("/// </param>");
		}

		if (m.ReturnValue.Comments.Any())
		{
			output.AppendLine("/// <return>");
			foreach (var s in Normalize(m.ReturnValue.Comments)) output.AppendLine($"/// {s}");
			output.AppendLine("/// </return>");
		}

		return output.ToString();
	}

	public static string SerializeMethodComments(ConvertedMethod m)
	{
		if (m.Comments == null) return "";

		var output = new StringBuilder();
		output.AppendLine(SerializeSummaryComments(m.Comments));

		foreach (var p in m.Parameters)
		{
			output.AppendLine($"/// <param name=\"{p.Name}\">");
			foreach (var s in Normalize(p.Comments)) output.AppendLine($"/// {s}");
			output.AppendLine("/// </param>");
		}

		if (m.ReturnValue.Comments.Any())
		{
			output.AppendLine("/// <return>");
			foreach (var s in Normalize(m.ReturnValue.Comments)) output.AppendLine($"/// {s}");
			output.AppendLine("/// </return>");
		}

		return output.ToString();
	}
}
