namespace BindingTransform;

public class CDeclParser
{
	private readonly CDeclLexer _lexer;

	public CDeclParser(CDeclLexer lexer)
	{
		_lexer = lexer;
	}

	public MethodDeclaration ReadMethodDeclaration()
	{
		var returnType = ReadType();
		var name = _lexer.Next(TokenType.Identifier);
		var parameters = ReadMethodParameters();
		return new MethodDeclaration() { Name = name.Text, ReturnType = returnType, Parameters = parameters };
	}

	public List<MethodParameter> ReadMethodParameters()
	{
		var result = new List<MethodParameter>();
		_lexer.Next(TokenType.ParamStart);

		while (_lexer.PeekNext().TokenType != TokenType.ParamEnd)
		{
			var p = ReadMethodParameter();
			if (p != null) result.Add(p);
			if (_lexer.PeekNext().TokenType == TokenType.Comma) _lexer.Next(TokenType.Comma);
		}
		_lexer.Next(TokenType.ParamEnd);
		return result;
	}

	public MethodParameter ReadMethodParameter()
	{
		if (_lexer.PeekNext().TokenType == TokenType.VarArgs)
		{
			_lexer.Next(TokenType.VarArgs);
			return new MethodParameter() { Name = "...", IsVarArgs = true };
		}

		var type = ReadType();
		var whitespace = _lexer.SkipWhitespace();
		if (whitespace.Contains("\n")) return null;
		var identifier = _lexer.Next(TokenType.Identifier);
		return new MethodParameter() { Type = type, Name = identifier.Text };
	}

	public StructDeclaration ReadStructDeclaration()
	{
		var structKeyword = _lexer.Next();

		if (structKeyword.TokenType != TokenType.StructKeyword && structKeyword.TokenType != TokenType.UnionKeyword)
		{
			throw new Exception("Unexpected token: " + structKeyword);
		}

		var identifier = _lexer.Next(TokenType.Identifier);
		_lexer.Next(TokenType.BlockStart);

		if (_lexer.PeekNext().TokenType == TokenType.Comment)
		{
			_lexer.Next(TokenType.Comment);
			_lexer.Next(TokenType.BlockEnd);
			return new StructDeclaration() { Name = identifier.Text };
		}

		var props = new List<StructProperty>();
		while (_lexer.PeekNext().TokenType != TokenType.BlockEnd) props.Add(ReadStructProperty());
		_lexer.Next(TokenType.BlockEnd);
		return new StructDeclaration() { Name = identifier.Text, Properties = props };
	}

	public StructProperty ReadStructProperty()
	{
		var property = new StructProperty();
		if (_lexer.PeekNext().TokenType == TokenType.VolatileKeyword) _lexer.Next(TokenType.VolatileKeyword);
		var parsedType = ReadType();

		if (_lexer.PeekNext().TokenType == TokenType.ParamStart)
		{
			_lexer.Pushback(parsedType.Tokens);
			property.Func = ReadFunctionPointer();
			property.Name = property.Func.Name;
		}
		else
		{
			property.Name = _lexer.Next(TokenType.Identifier).Text;
			property.Type = parsedType;
		}

		if (_lexer.PeekNext().TokenType == TokenType.Colon)
		{
			_lexer.Next(TokenType.Colon);
			property.Bits = int.Parse(_lexer.Next(TokenType.Number).Text);
		}

		_lexer.Next(TokenType.EndStatement);
		return property;
	}

	public MethodDeclaration ReadFunctionPointer()
	{
		var functionPointer = new MethodDeclaration();
		functionPointer.ReturnType = ReadType();
		_lexer.Next(TokenType.ParamStart);
		_lexer.Next(TokenType.Star);
		functionPointer.Name = _lexer.Next(TokenType.Identifier).Text;
		_lexer.Next(TokenType.ParamEnd);
		functionPointer.Parameters = ReadMethodParameters();
		return functionPointer;
	}

	private ParsedType ReadType()
	{
		var tokens = new List<Token>();

		if (_lexer.PeekNext().TokenType == TokenType.VolatileKeyword)
		{
			_ = _lexer.Next(TokenType.VolatileKeyword);
		}

		if (_lexer.PeekNext().TokenType == TokenType.ConstKeyword)
		{
			tokens.Add(_lexer.Next(TokenType.ConstKeyword));
			tokens.AddRange(ReadPointers());
		}

		var identifiers = ReadIdentifier();
		tokens.AddRange(identifiers);
		tokens.AddRange(ReadPointers());

		if (_lexer.PeekNext().TokenType == TokenType.ConstKeyword)
		{
			tokens.Add(_lexer.Next(TokenType.ConstKeyword));
			tokens.AddRange(ReadPointers());
		}
		else
		{
			if (identifiers.Last().Text == "unsigned" && _lexer.PeekNext().Text == "int") tokens.Add(_lexer.Next());
			else if (_lexer.PeekNext().Text == "double") tokens.Add(_lexer.Next());

			tokens.AddRange(ReadPointers());
		}

		if (_lexer.PeekNext().TokenType == TokenType.ConstKeyword)
		{
			tokens.Add(_lexer.Next(TokenType.ConstKeyword));
			tokens.AddRange(ReadPointers());
		}

		return new ParsedType() { Tokens = tokens };
	}

	private List<Token> ReadIdentifier()
	{
		var tokens = new List<Token>();
		tokens.Add(_lexer.Next(TokenType.Identifier));

		while (_lexer.PeekNext().TokenType == TokenType.Period)
		{
			tokens.Add(_lexer.Next(TokenType.Period));

			if (_lexer.PeekNext().TokenType == TokenType.Identifier)
			{
				tokens.Add(_lexer.Next(TokenType.Identifier));
			}
		}

		return tokens;
	}

	private List<Token> ReadPointers()
	{
		var tokens = new List<Token>();
		while (_lexer.PeekNext().TokenType == TokenType.Star) tokens.Add(_lexer.Next());
		return tokens;
	}

	public AliasDeclaration ReadAlias()
	{
		_lexer.Next(TokenType.TypedefKeyword);
		var type = ReadType();
		var name = _lexer.Next(TokenType.Identifier).Text;
		return new AliasDeclaration() { Name = name, Type = type };
	}
}
