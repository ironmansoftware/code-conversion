using System;
using System.Collections.Generic;

namespace CodeConverter.Common
{
    /// <summary>
    /// Abstract node in a syntax tree.
    /// </summary>
    public abstract class Node
    {
		public Node Parent { get; set; }
        public static CodeWriter DefaultCodeWriter { get; set; }
        public CodeWriter CodeWriter { get; set; }
        public Language SourceLanguage { get; }
        public string OriginalSource { get; set; }
		public Intent Intent { get; set; }
        public abstract void Accept(NodeVisitor visitor);

		protected void SetParent(Node node)
		{
			if (node != null)
				node.Parent = this;
		}

		protected void SetParent(IEnumerable<Node> nodes)
		{
			if (nodes != null)
			{
				foreach(var node in nodes)
				{
					node.Parent = this;
				}
			}
		}

        public override string ToString()
        {
            if (CodeWriter != null) return CodeWriter.Write(this);
            if (DefaultCodeWriter != null) return DefaultCodeWriter.Write(this);
            return GetType().ToString();
        }
    }
    
    /// <summary>
    /// Binary operators.
    /// </summary>
    public enum BinaryOperator
    {
        Unknown,
        NotEqual,
        Equal,
        Not,
        GreaterThan,
        GreaterThanEqualTo,
        LessThan,
        LessThanEqualTo,
        Or,
        And,
        Bor,
        Minus,
        Plus
    }

	public class AnonymousType : Node
	{
		public AnonymousType(IEnumerable<AnonymousTypeMember> members)
		{
			Members = members;
			SetParent(members);
		}

		public IEnumerable<AnonymousTypeMember> Members { get; }

		public override void Accept(NodeVisitor visitor)
		{
			visitor.VisitAnonymousType(this);
		}
	}

	public class AnonymousTypeMember : Node
	{
		public AnonymousTypeMember(string name, Node value)
		{
			Name = name;
			Value = value;
			SetParent(value);
		}

		public string Name { get; set; }
		public Node Value { get; set; }

		public override void Accept(NodeVisitor visitor)
		{
			visitor.VisitAnonynmousTypeMember(this);
		}
	}

    public class ArrayCreation : Node
    {
        public ArrayCreation(IEnumerable<Node> initializers, string type)
        {
            Initializer = initializers;
            Type = type;
			SetParent(Initializer);
        }

        public IEnumerable<Node> Initializer { get; }
        public string Type { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitArrayCreation(this);
        }
    }

    /// <summary>
    /// Binary expression. 
    /// </summary>
    public class BinaryExpression : Node {
        public BinaryExpression(Node left, BinaryOperator @operator, Node right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
			SetParent(Left);
			SetParent(Right);
        }

        public Node Left { get; }
        public BinaryOperator Operator { get; }
        public Node Right { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitBinaryExpression(this);
        }
    }

    /// <summary>
    /// Assignment
    /// </summary>
    public class Assignment : Node
    {
        public Assignment(Node left, Node right)
        {
            Left = left;
            Right = right;
			SetParent(Left);
			SetParent(Right);
        }

        public Node Left { get; }
        public Node Right { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitAssignment(this);
        }
    }
    
    public class Argument : Node
    {
        public Argument(Node expression)
        {
            Expression = expression;
			SetParent(Expression);
        }

		public Argument(string name, Node expression)
		{
			Name = name;
			Expression = expression;
			SetParent(Expression);
		}

		public string Name { get; }
        public Node Expression { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitArgument(this);
        }
    }

    public class ArgumentList : Node
    {
        public ArgumentList(params Node[] arguments)
        {
            Arguments = arguments;
			SetParent(Arguments);
        }

        public ArgumentList(IEnumerable<Node> arguments)
        {
            Arguments = arguments;
        }

        public IEnumerable<Node> Arguments { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitArgumentList(this);
        }
    }

    public class Attribute : Node
    {
        public Attribute(string name, ArgumentList argumentList)
        {
            ArgumentList = argumentList;
            Name = name;
			SetParent(ArgumentList);
        }

        public ArgumentList ArgumentList { get; set; }
        public string Name { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitAttribute(this);
        }
    }

    public class Block : Node
    { 
        public Block(params Node[] statements)
        {
            Statements = statements;
			SetParent(Statements);
        }

        public Block(IEnumerable<Node> statements)
        {
            Statements = statements;
        }

        public IEnumerable<Node> Statements { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitBlock(this);
        }
    }

    public class BracketedArgumentList : ArgumentList
    {
        public BracketedArgumentList(ArgumentList argumentList) : base(argumentList)
        {
        }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitBracketedArgumentList(this);
        }
    }

    public class Break : Node
    {
        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitBreak(this);
        }
    }

    public class Cast : Node
    {
        public Cast(string type, Node expression)
        {
            Type = type;
            Expression = expression;
			SetParent(Expression);
		}
        public string Type { get; }
        public Node Expression { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitCast(this);
        }
    }

    public class Catch : Node
    {
        public Catch(Node declaration, Block block)
        {
            Declaration = declaration;
            Block = block;
			SetParent(Declaration);
			SetParent(Block);
		}

        public Node Declaration { get; }
        public Block Block { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitCatch(this);
        }
    }

    public class CatchDeclaration : Node
    {
        public CatchDeclaration(string type, string identifier)
        {
            Type = type;
            Identifier = identifier;
        }

        public string Type { get; }
        public string Identifier { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitCatchDeclaration(this);
        }
    }
    
    public class ClassDeclaration : Node
    {
        public string Name { get; }
        public IEnumerable<string> Bases { get; }
        public IEnumerable<Node> Members { get; }

        public ClassDeclaration(string name, params Node[] members)
        {
            Name = name;
            Members = members;
			SetParent(Members);
		}

        public ClassDeclaration(string name, IEnumerable<Node> members, IEnumerable<string> bases = null)
        {
            Name = name;
            Members = members;
            Bases = bases;
			SetParent(members);
		}

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitClassDeclaration(this);
        }
    }

    public class Continue : Node
    {
        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitContinue(this);
        }
    }

    public class Constructor : Node
    {
        public ArgumentList ArgumentList { get; }
        public Node Body { get; }
        public string Identifier { get; }

        public Constructor(string identifier, Node body, ArgumentList argumentList)
        {
            ArgumentList = argumentList;
            Body = body;
            Identifier = identifier;

            SetParent(ArgumentList);
            SetParent(Body);
        }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitConstructor(this);
        }
    }


    public class ElementAccess : Node
    {
        public ElementAccess(Node expression, ArgumentList argumentList)
        {
            Expression = expression;
            ArgumentList = argumentList;
			SetParent(Expression);
			SetParent(ArgumentList);
		}
        public Node Expression { get; }
        public ArgumentList ArgumentList { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitElementAccess(this);
        }
    }

    public class ElseClause : Node
    {
        public ElseClause(Node body)
        {
            Body = body;
			SetParent(Body);
		}

        public Node Body { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitElseClause(this);
        }
    }

    public class ForStatement : Node
    {
        public ForStatement(Node declaration, IEnumerable<Node> initializers, IEnumerable<Node> incrementors, Node condition, Node statement)
        {
            Declaration = declaration;
            Initializers = initializers;
            Condition = condition;
            Incrementors = incrementors;
            Statement = statement;
			SetParent(Declaration);
			SetParent(Initializers);
			SetParent(Incrementors);
			SetParent(Condition);
			SetParent(Statement);
		}

		public ForStatement(Node initializer, Node incrementor, Node condition, Node statement)
		{
			Initializers = new[] { initializer };
			Condition = condition;
			Incrementors = new[] { incrementor };
			Statement = statement;

			SetParent(Declaration);
			SetParent(Initializers);
			SetParent(Incrementors);
			SetParent(Condition);
			SetParent(Statement);
		}

		public Node Declaration { get; }
        public IEnumerable<Node> Initializers { get; }
        public IEnumerable<Node> Incrementors { get; }
        public Node Condition { get; }
        public Node Statement { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitForStatement(this);
        }
    }

    public class FieldDeclaration : Node
    {
        public FieldDeclaration(string type, string name, string defaultValue, IEnumerable<string> modifiers)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Modifiers = modifiers;
        }
        public string Name { get; }
        public string Type { get; }
        public string DefaultValue { get; }
        public IEnumerable<string> Modifiers { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitFieldDeclaration(this);
        }
    }


    public class Finally : Node
    {
        public Finally(Node body)
        {
            Body = body;
			SetParent(Body);
		}

        public Node Body { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitFinally(this);
        }
    }
    
    public class ForEachStatement : Node
    {
        public ForEachStatement(IdentifierName identifier, Node expression, Node statement)
        {
            Identifier = identifier;
            Expression = expression;
            Statement = statement;

			SetParent(Identifier);
			SetParent(Expression);
			SetParent(Statement);
		}

        public IdentifierName Identifier { get; }
        public Node Expression { get; }
        public Node Statement { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitForEachStatement(this);
        }
    }

    public class IdentifierName : Node
    {
        public IdentifierName(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitIdentifierName(this);
        }
    }

    public class IfStatement : Node
    {
        public IfStatement(Node condition, Node body)
        {
            Condition = condition;
            Body = body;

			SetParent(Condition);
			SetParent(Body);
		}

        public IfStatement(Node condition, Node body, ElseClause elseClause)
        {
            Condition = condition;
            Body = body;
            ElseClause = elseClause;

			SetParent(Condition);
			SetParent(Body);
			SetParent(ElseClause);
		}

        public Node Condition { get; }
        public Node Body { get; }
        public ElseClause ElseClause { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitIfStatement(this);
        }
    }

    public class Literal : Node
    {
        public Literal(string token)
        {
            Token = token;
        }
        public string Token { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitLiteral(this);
        }
    }

    public class Invocation : Node
    {
        public Invocation(Node expression, ArgumentList arguments)
        {
            Expression = expression;
            Arguments = arguments;

			SetParent(Expression);
			SetParent(Arguments);
		}

        public Node Expression { get; }
        public ArgumentList Arguments { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitInvocation(this);
        }
    }

    public class MemberAccess : Node
    {
        public MemberAccess(Node expression, string identifier)
        {
            Expression = expression;
            Identifier = identifier;

			SetParent(Expression);
		}

        public Node Expression { get; }
        public string Identifier { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitMemberAccess(this);
        }
    }

    public class MethodDeclaration : Node
    {
        public MethodDeclaration(string name, IEnumerable<Parameter> parameters, Node body, IEnumerable<string> modifiers, IEnumerable<Attribute> attributes, string returnType)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
            Modifiers = modifiers;
            Attributes = attributes;
            ReturnType = returnType;

			SetParent(Parameters);
			SetParent(Body);
			SetParent(Attributes);
		}

        public string Name { get; }
        public IEnumerable<Parameter> Parameters { get; set; }
        public Node Body { get; }
        public IEnumerable<string> Modifiers { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }
        public string ReturnType { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitMethodDeclaration(this);
        }
    }

    public class Namespace : Node
    {
        public Namespace(string name, params Node[] members)
        {
            Name = name;
            Members = members;

			SetParent(Members);
		}

        public Namespace(string name, IEnumerable<Node> members)
        {
            Name = name;
            Members = members;
        }

        public string Name { get; }
        public IEnumerable<Node> Members { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitNamespace(this);
        }
    }

    public class ObjectCreation : Node
    {
        public ObjectCreation(string type, ArgumentList arguments)
        {
            Type = type;
            Arguments = arguments;

			SetParent(Arguments);
		}

        public string Type { get; }
        public ArgumentList Arguments { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitObjectCreation(this);
        }
    }

    public class Parameter : Node
    {
        public Parameter(string type, string name)
        {
            Type = type;
            Name = name;
            Modifiers = new string[0];
        }

        public Parameter(string type, string name, IEnumerable<string> modifiers)
        {
            Type = type;
            Name = name;
            Modifiers = modifiers;
        }

        public string Type { get; }
        public string Name { get; }
        public IEnumerable<string> Modifiers { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitParameter(this);
        }
    }

    public class ParenthesizedExpression : Node
    {
        public ParenthesizedExpression(Node expression)
        {
            Expression = expression;

			SetParent(Expression);
		}

        public Node Expression { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitParenthesizedExpression(this);
        }
    }

	public class PipelineExpression : Node
	{
		public PipelineExpression(IEnumerable<Node> elements)
		{
			Elements = elements;
			SetParent(Elements);
		}

		public IEnumerable<Node> Elements { get; }

		public override void Accept(NodeVisitor visitor)
		{
			visitor.VisitPipelineExpression(this);
		}
	}


    public class PostfixUnaryExpression : Node
    {
        public PostfixUnaryExpression(Node operand, string @operator)
        {
            Operand = operand;
            Operator = @operator;

			SetParent(Operand);
		}
        public Node Operand { get; }
        public string Operator { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitPostfixUnaryExpression(this);
        }
    }

    public class PrefixUnaryExpression : Node
    {
        public PrefixUnaryExpression(Node operand, string @operator)
        {
            Operand = operand;
            Operator = @operator;

			SetParent(Operand);
		}
        public Node Operand { get; }
        public string Operator { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitPrefixUnaryExpression(this);
        }
    }

    public class PropertyDeclaration : Node
    {
        public PropertyDeclaration(string type, string name, string defaultValue, IEnumerable<string> modifiers)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Modifiers = modifiers;
        }
        public string Name { get; }
        public string Type { get; }
        public string DefaultValue { get; }
        public IEnumerable<string> Modifiers { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitPropertyDeclaration(this);
        }
    }

    public class StringConstant : Node
    {
        public StringConstant(string value)
        {
            Value = value;
        }
        public string Value { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitStringConstant(this);
        }
    }

    public class TemplateStringConstant : Node
    {
        public TemplateStringConstant(string value)
        {
            Value = value;
        }
        public string Value { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitTemplateStringConstant(this);
        }
    }

    public class ThisExpression : Node
    {
        public ThisExpression()
        {
        }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitThisExpression(this);
        }
    }

    public class Throw :Node
    {
        public Throw(Node statement)
        {
            Statement = statement;

			SetParent(Statement);
		}

        public Node Statement { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitThrow(this);
        }
    }
    
    public class Try : Node
    {
		public Try(Block block, IEnumerable<Catch> catches)
		{
			Block = block;
			Catches = catches;

			SetParent(Block);
			SetParent(Catches);
		}
		public Try(Block block, IEnumerable<Catch> catches, Finally @finally)
        {
            Block = block;
            Catches = catches;
            Finally = @finally;

			SetParent(Block);
			SetParent(Catches);
			SetParent(Finally);
		}
        public Block Block { get; }
        public IEnumerable<Catch> Catches { get; }
        public Finally Finally { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitTry(this);
        }
    }

	public class RawCode : Node
	{
		public RawCode(string code)
		{
			Code = code;
		}

		public string Code { get; }

		public override void Accept(NodeVisitor visitor)
		{
			visitor.VisitRawCode(this);
		}
	}

    public class ReturnStatement : Node
    {
        public ReturnStatement(Node expression)
        {
            Expression = expression;

			SetParent(Expression);
		}

        public Node Expression { get; }
        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitReturnStatement(this);
        }
    }

    public class SwitchStatement : Node
    {
        public SwitchStatement(Node expression, IEnumerable<SwitchSection> sections)
        {
            Expression = expression;
            Sections = sections;

			SetParent(Expression);
			SetParent(Sections);
		}

        public Node Expression { get; set; }
        public IEnumerable<SwitchSection> Sections { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitSwitchStatement(this);
        }
    }

    public class SwitchSection : Node
    {
        public SwitchSection(IEnumerable<Node> labels, IEnumerable<Node> statements)
        {
            Labels = labels;
            Statements = statements;

			SetParent(Labels);
			SetParent(Statements);
		}

        public IEnumerable<Node> Labels { get; set; }
        public IEnumerable<Node> Statements { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitSwitchSection(this);
        }
    }

	public class TypeExpression : Node
	{
		public TypeExpression(string typeName)
		{
			TypeName = typeName;
		}

		public string TypeName { get; set; }

		public override void Accept(NodeVisitor visitor)
		{
			visitor.VisitTypeExpression(this);
		}
	}

    public class Unknown : Node
    {
        public Unknown(string message)
        {
            Message = message;
        }

        public string Message { get; set; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitUnknown(this);
        }
    }

    public class Using : Node
    {
        public Using(Node expression, Node declaration)
        {
            Declaration = declaration;
            Expression = expression;

			SetParent(Declaration);
			SetParent(Expression);
		}
        public Node Declaration { get; }
        public Node Expression { get; }
        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitUsing(this);
        }
    }

    public class VariableDeclaration : Node
    {
        public VariableDeclaration(string type, params VariableDeclarator[] variables)
        {
            Type = type;
            Variables = variables;

			SetParent(Variables);
		}

        public VariableDeclaration(string type, IEnumerable<VariableDeclarator> variables)
        {
            Type = type;
            Variables = variables;

			SetParent(Variables);
		}
        public string Type { get; }
        public IEnumerable<VariableDeclarator> Variables { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitVariableDeclaration(this);
        }
    }

    public class VariableDeclarator : Node
    {
        public VariableDeclarator(string name, Node initializer)
        {
            Name = name;
            Initializer = initializer;

			SetParent(Initializer);
		}

        public string Name { get; }
        public Node Initializer { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitVariableDeclarator(this);
        }
    }

    public class While : Node
    {
        public While(Node condition, Block statement)
        {
            Condition = condition;
            Statement = statement;

			SetParent(Condition);
			SetParent(Statement);
		}

        public Node Condition { get; }
        public Block Statement { get; }

        public override void Accept(NodeVisitor visitor)
        {
            visitor.VisitWhile(this);
        }
    }
}
