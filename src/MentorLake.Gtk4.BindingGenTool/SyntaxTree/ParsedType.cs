using System.Text;
using System.Text.RegularExpressions;

namespace BindingTransform;

public class ParsedType
{
	public List<Token> Tokens { get; set; }

	public string ToCString()
	{
		var output = new StringBuilder();
		IEnumerable<Token> remainingTokens = Tokens;

		if (Tokens.First().TokenType == TokenType.ConstKeyword)
		{
			remainingTokens = remainingTokens.Skip(1);
			var pointerTokens = remainingTokens.TakeWhile(t => t.TokenType == TokenType.Star).Select(t => "*").ToList();
			remainingTokens = remainingTokens.Skip(pointerTokens.Count);

			output.Append("const");
			output.Append(string.Join("", pointerTokens));
			output.Append(" ");
		}

		var name = remainingTokens.First();
		remainingTokens = remainingTokens.Skip(1);

		if (name.Text == "long" &&  remainingTokens.Any() && remainingTokens.First().Text == "double")
		{
			output.Append("decimal");
			remainingTokens = remainingTokens.Skip(2);
		}
		else
		{
			output.Append(name.Text);
		}

		output.Append(string.Join("", remainingTokens.TakeWhile(t => t.TokenType == TokenType.Star).Select(t => "*").ToList()));
		return output.ToString();
	}
}
