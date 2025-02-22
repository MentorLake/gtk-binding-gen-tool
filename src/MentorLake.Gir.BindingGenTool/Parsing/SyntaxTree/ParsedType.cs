using System.Text;

namespace BindingTransform;

public class ParsedType
{
	public List<Token> Tokens { get; set; }

	public string ToCString()
	{
		var output = new StringBuilder();
		var tokenQueue = new Queue<Token>();
		Tokens.ForEach(tokenQueue.Enqueue);

		if (tokenQueue.Peek().TokenType == TokenType.ConstKeyword)
		{
			tokenQueue.Dequeue();
			var pointerTokens = new List<Token>();
			while (tokenQueue.Peek().TokenType == TokenType.Star) pointerTokens.Add(tokenQueue.Dequeue());
			output.Append("const");
			output.Append(string.Join("", pointerTokens));
			output.Append(" ");
		}

		var name = tokenQueue.Dequeue();

		if (name.Text == "long" &&  tokenQueue.Any() && tokenQueue.Peek().Text == "double")
		{
			output.Append("decimal");
			tokenQueue.Dequeue();
		}
		else
		{
			output.Append(name.Text);

			while (tokenQueue.Any() && tokenQueue.Peek().TokenType == TokenType.Period)
			{
				output.Append(tokenQueue.Dequeue().Text);
				if (tokenQueue.Peek().TokenType == TokenType.Identifier) output.Append(tokenQueue.Dequeue().Text);
			}
		}

		var remainingPointers = new List<string>();
		while (tokenQueue.Any() && tokenQueue.Peek().TokenType == TokenType.Star) remainingPointers.Add(tokenQueue.Dequeue().Text);
		output.Append(string.Join("", remainingPointers));
		return output.ToString();
	}
}
