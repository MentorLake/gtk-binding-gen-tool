using System.Text;

namespace BindingTransform;

public enum TokenType : int
{
	StructKeyword,
	UnionKeyword,
	ConstKeyword,
	Identifier,
	BlockStart,
	BlockEnd,
	ParamStart,
	ParamEnd,
	Star,
	Comment,
	EOF,
	EndStatement,
	Comma,
	Colon,
	Number,
	VolatileKeyword,
	TypedefKeyword,
	VarArgs
}

public record Token
{
	public string Text { get; set; }
	public TokenType TokenType { get; set; }
}

public class CDeclLexer(string source)
{
	private int _currentIndex = 0;
	private readonly Queue<Token> _pushedBackTokens = new();

	public Token PeekNext()
	{
		var priorIndex = _currentIndex;
		var token = Next();
		_currentIndex = priorIndex;
		return token;
	}

	public List<Token> PeekNext(int count)
	{
		var priorIndex = _currentIndex;
		var result = new List<Token>();
		for (var i=0; i<count; i++) result.Add(Next());
		_currentIndex = priorIndex;
		return result;
	}

	public Token Next()
	{
		if (_pushedBackTokens.Any()) return _pushedBackTokens.Dequeue();

		SkipWhitespace();

		while (_currentIndex < source.Length)
		{
			var currentChar = source[_currentIndex++].ToString();

			if (currentChar == "*") return new Token() { TokenType = TokenType.Star, Text = currentChar };
			if (currentChar == ";") return new Token() { TokenType = TokenType.EndStatement, Text = currentChar };
			if (currentChar == "(")  return new Token() { TokenType = TokenType.ParamStart, Text = currentChar };
			if (currentChar == ")")  return new Token() { TokenType = TokenType.ParamEnd, Text = currentChar };
			if (currentChar == "{")  return new Token() { TokenType = TokenType.BlockStart, Text = currentChar };
			if (currentChar == "}")  return new Token() { TokenType = TokenType.BlockEnd, Text = currentChar };
			if (currentChar == ",")  return new Token() { TokenType = TokenType.Comma, Text = currentChar };
			if (currentChar == ":")  return new Token() { TokenType = TokenType.Colon, Text = currentChar };
			if (currentChar == "." && PeekChars(2) == "..") return new Token() { TokenType = TokenType.VarArgs, Text = "." + ReadChars(2) };
			if (char.IsNumber(currentChar[0]))  return new Token() { TokenType = TokenType.Number, Text = ReadNumber() };
			if (currentChar == "/" && PeekChars(1) == "*") return new Token() { TokenType = TokenType.Comment, Text = ReadComment() };
			if (currentChar == "u" && TryReadWord("nion")) return new Token() { TokenType = TokenType.StructKeyword, Text = "u" + ReadWord() };
			if (currentChar == "s" && TryReadWord("truct")) return new Token() { TokenType = TokenType.StructKeyword, Text = "s" + ReadWord() };
			if (currentChar == "c" && TryReadWord("onst")) return new Token() { TokenType = TokenType.ConstKeyword, Text = "c" + ReadWord() };
			if (currentChar == "v" && TryReadWord("olatile")) return new Token() { TokenType = TokenType.VolatileKeyword, Text = "v" + ReadWord() };
			if (currentChar == "t" && TryReadWord("ypedef")) return new Token() { TokenType = TokenType.TypedefKeyword, Text = "t" + ReadWord() };

			_currentIndex--;
			return new Token() { TokenType = TokenType.Identifier, Text = ReadWord() };
		}

		return new Token() { TokenType = TokenType.EOF, Text = "" };
	}
	public void Pushback(List<Token> tokens)
	{
		tokens.ForEach(t => _pushedBackTokens.Enqueue(t));
	}

	private string ReadNumber()
	{
		_currentIndex--;
		var output = new StringBuilder();
		while (_currentIndex < source.Length && char.IsNumber(source[_currentIndex])) output.Append(source[_currentIndex++]);
		return output.ToString();
	}

	private string ReadComment()
	{
		_currentIndex--;
		var output = new StringBuilder();
		while (_currentIndex < source.Length && !output.ToString().EndsWith("*/")) output.Append(source[_currentIndex++]);
		return output.ToString();
	}

	private bool TryReadWord(string s)
	{
		var priorIndex = _currentIndex;
		var word = ReadWord();
		_currentIndex = priorIndex;
		return word == s;
	}

	private string ReadWord()
	{
		var output = new StringBuilder();
		while (_currentIndex < source.Length && IsValidWordChar(source[_currentIndex])) output.Append(source[_currentIndex++]);
		return output.ToString();
	}

	private bool IsValidWordChar(char c)
	{
		return Char.IsLetter(c) || Char.IsNumber(c) || c == '_';
	}

	private string PeekChars(int count)
	{
		var output = new StringBuilder();
		for (var i = _currentIndex; i < count + _currentIndex && i < source.Length; i++) output.Append(source[i]);
		return output.ToString();
	}

	public Token Next(TokenType expectedType)
	{
		var result = Next();
		if (result.TokenType != expectedType) throw new Exception("Unexpected token: " + result.TokenType + " " + result.Text);
		return result;
	}

	public string SkipWhitespace()
	{
		var output = new StringBuilder();
		while (_currentIndex < source.Length && source[_currentIndex] is ' ' or '\t' or '\n' or '\r') output.Append(source[_currentIndex++]);
		return output.ToString();
	}

	public string ReadChars(int count)
	{
		var s = "";
		for (var i = 0; i < count; i++)
		{
			s += source[_currentIndex++];
		}

		return s;
	}
}
