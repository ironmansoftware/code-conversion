using CodeConverter.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace CodeConverter.CSharp
{
    public class CSharpSyntaxTreeVisitor : ISyntaxTreeVisitor
    {
        private bool _secondAttempt;
        public Language Language => Language.CSharp;

        public Node Visit(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var root = tree.GetCompilationUnitRoot();
            var visitor = new Visitor();

            try
            {
                root.Accept(visitor);
            }
            catch (IncompleteCodeBlockException)
            {
                if (_secondAttempt)
                {
                    throw new System.Exception("Invalid C# code specified.");
                }
                _secondAttempt = true;
                return Visit($"void Method() {{ \r\n { code } \r\n }}");
            }

            return visitor.Node;
        }
    }

    internal class Visitor : CSharpSyntaxVisitor
    {
        private Node _currentNode;
        private static Dictionary<string, BinaryOperator> _operatorMap;

        static Visitor()
        {
            _operatorMap = new Dictionary<string, BinaryOperator>
            {
                { "==", BinaryOperator.Equal },
                { "!=", BinaryOperator.NotEqual },
                { ">", BinaryOperator.GreaterThan },
                { ">=", BinaryOperator.GreaterThanEqualTo },
                { "<", BinaryOperator.LessThan},
                { "<=", BinaryOperator.LessThanEqualTo},
                { "&&", BinaryOperator.And},
                { "||", BinaryOperator.Or},
                { "|", BinaryOperator.Bor},
                { "-", BinaryOperator.Minus },
                { "+", BinaryOperator.Plus },
                { "!", BinaryOperator.Not }
            };
        }

        public Node Node => _currentNode;

        public Node VisitSyntaxNode(CSharpSyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

			var originalSource = node.ToString();

            Visit(node);

            if (_currentNode == null)
                _currentNode = new Unknown($"Unsupported node: {node.GetType().Name}");

            _currentNode.OriginalSource = originalSource;

            return _currentNode;
        }

        public override void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            var elements = new List<Node>();
            if (node.Initializer != null)
            {
                foreach(var expression in node.Initializer.Expressions)
                    elements.Add(VisitSyntaxNode(expression));
            }

            var type = node.Type.GetText().ToString().Trim();
            _currentNode = new ArrayCreation(elements, type);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var left = VisitSyntaxNode(node.Left);
            var right = VisitSyntaxNode(node.Right);

            _currentNode = new Assignment(left, right);
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            var list = VisitSyntaxNode(node.ArgumentList) as ArgumentList;

            _currentNode = new Attribute(node.Name.ToString(), list);
        }

        public override void VisitArgument(ArgumentSyntax node)
        {
            var expression = VisitSyntaxNode(node.Expression);

            _currentNode = new Argument(expression);
        }

        public override void VisitArgumentList(ArgumentListSyntax node)
        {
            var arguments = new List<Node>();
            foreach (var argument in node.Arguments)
            {
                var argumentNode = VisitSyntaxNode(argument);

                arguments.Add(argumentNode);
            }

            _currentNode = new ArgumentList(arguments);
        }

        public override void VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            //Visit(node.Expression);
            //_builder.Append(".Result");
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var left = VisitSyntaxNode(node.Left);
            string op = node.OperatorToken.Text;

            var @operator = BinaryOperator.Unknown;

            if (_operatorMap.ContainsKey(op))
            {
                @operator = _operatorMap[op];
            }

            var right = VisitSyntaxNode(node.Right);

            _currentNode = new BinaryExpression(left, @operator, right);
        }

        public override void VisitBracketedArgumentList(BracketedArgumentListSyntax node)
        {
            var arguments = new List<Argument>();
            foreach(var argument in node.Arguments)
            {
                arguments.Add(VisitSyntaxNode(argument) as Argument);
            }

            _currentNode = new BracketedArgumentList(new ArgumentList(arguments));
        }

        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            _currentNode = new Break();
        }

        public override void VisitBlock(BlockSyntax node)
        {
            var statements = new List<Node>();
            foreach (var statement in node.Statements)
            {
                var statementNode = VisitSyntaxNode(statement);
                statements.Add(statementNode);
            }

            _currentNode = new Block(statements);
        }

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            var expresion = VisitSyntaxNode(node.Expression);

            _currentNode = new Cast(node.Type.GetText().ToString().Trim(), expresion);
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            Node declaration = null;
            if (node.Declaration != null)
            {
                declaration = VisitSyntaxNode(node.Declaration);
            }

            var block = VisitSyntaxNode(node.Block) as Block;
            _currentNode = new Catch(declaration, block);
        }

        public override void VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            var type = node.Type.ToString();
            var identifier = node.Identifier.ValueText;

            _currentNode = new CatchDeclaration(type, identifier);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var name = node.Identifier.ToString();
            var members = new List<Node>();
            foreach(var member in node.Members)
            {
                members.Add(VisitSyntaxNode(member));
            }

            var baseList = node.BaseList == null ? new string[0] : node.BaseList.Types.Select(m => m.Type.ToString());

            _currentNode = new ClassDeclaration(name, members, baseList);
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var nodes = new List<Node>();
            foreach(var member in node.Members)
            {
                nodes.Add(VisitSyntaxNode(member));
            }

            _currentNode = new Block(nodes);
        }

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            _currentNode = new Continue();
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var expression = VisitSyntaxNode(node.Body);
            var argumentList = VisitSyntaxNode(node.ParameterList) as ArgumentList;
            

            _currentNode = new Constructor(node.Identifier.ValueText, expression, argumentList);
        }

        public override void DefaultVisit(SyntaxNode node)
        {
            _currentNode = null;
            base.DefaultVisit(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var expression = VisitSyntaxNode(node.Expression);
            var argumentList = VisitSyntaxNode(node.ArgumentList) as ArgumentList;

            _currentNode = new ElementAccess(expression, argumentList);
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            var statement = VisitSyntaxNode(node.Statement);
            _currentNode = new ElseClause(statement);
        }

        public override void VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            _currentNode = VisitSyntaxNode(node.Value);
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            _currentNode = VisitSyntaxNode(node.Expression);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var field = node.Declaration.Variables.First();
            var modifiers = node.Modifiers.Select(m => m.ValueText);
            _currentNode = new FieldDeclaration(node.Declaration.Type.ToString(), field.Identifier.ValueText, null, modifiers);
        }

        public override void VisitFinallyClause(FinallyClauseSyntax node)
        {
            var body = VisitSyntaxNode(node.Block);

            _currentNode = new Finally(body);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            Node declaration = null;
            var initializers = new List<Node>();
            var incrementors = new List<Node>();
            Node condition = null;
            Node statement = null;

            if (node.Declaration != null)
            {
                declaration = VisitSyntaxNode(node.Declaration);
            }
            else
            {
                foreach (var initializer in node.Initializers)
                {
                    initializers.Add(VisitSyntaxNode(initializer));
                }
            }

            condition = VisitSyntaxNode(node.Condition);

            foreach (var incrementor in node.Incrementors)
            {
                incrementors.Add(VisitSyntaxNode(incrementor));
            }

            statement = VisitSyntaxNode(node.Statement);

            _currentNode = new ForStatement(declaration, initializers, incrementors, condition, statement);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            var expression = VisitSyntaxNode(node.Expression);
            var statement = VisitSyntaxNode(node.Statement);
            var identifier = new IdentifierName(node.Identifier.Text);

            _currentNode = new ForEachStatement(identifier, expression, statement);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            _currentNode = new IdentifierName(node.Identifier.ToString());
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            var condition = VisitSyntaxNode(node.Condition);
            var statement = VisitSyntaxNode(node.Statement);
            ElseClause elseClause = null;

            if (node.Else != null)
            {
                elseClause = VisitSyntaxNode(node.Else) as ElseClause;
            }

            _currentNode = new IfStatement(condition, statement, elseClause);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var expression = VisitSyntaxNode(node.Expression);
            var argumentList = VisitSyntaxNode(node.ArgumentList) as ArgumentList;
            _currentNode = new Invocation(expression, argumentList);
        }
        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            _currentNode = new Literal(node.Token.ToString());
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            _currentNode = VisitSyntaxNode(node.Declaration);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var expression = VisitSyntaxNode(node.Expression);
            var identifier = node.Name.ToString();
            _currentNode = new MemberAccess(expression, identifier);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var name = node.Identifier.ToString();
            var parameters = new List<Parameter>();
            foreach (var parameter in node.ParameterList.Parameters)
            {
                parameters.Add(VisitSyntaxNode(parameter) as Parameter);
            }

            var modifiers = node.Modifiers.Select(m => m.ToString());

            var body = VisitSyntaxNode(node.Body);

            var attributes = new List<Attribute>();
            foreach (var attribute in node.AttributeLists.SelectMany(m => m.Attributes))
            {
                var attributeNode = VisitSyntaxNode(attribute) as Attribute;
                if (attributeNode != null)
                    attributes.Add(attributeNode);
            }

            _currentNode = new MethodDeclaration(name, parameters, body, modifiers, attributes, node.ReturnType.ToString());
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var members = new List<Node>();
            foreach(var member in node.Members)
            {
                members.Add(VisitSyntaxNode(member));
            }

            _currentNode = new Namespace(node.Name.ToString(), members);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var type = node.Type.ToString();
            var argumentList = VisitSyntaxNode(node.ArgumentList) as ArgumentList;
            _currentNode = new ObjectCreation(type, argumentList);
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            var modifiers = node.Modifiers.Select(m => m.ToString());

            _currentNode = new Parameter(node.Type.ToString(), node.Identifier.ToString(), modifiers);
        }

        public override void VisitParameterList(ParameterListSyntax node)
        {
            var arguments = new List<Node>();
            foreach (var argument in node.Parameters)
            {
                var argumentNode = VisitSyntaxNode(argument);

                arguments.Add(argumentNode);
            }

            _currentNode = new ArgumentList(arguments);
        }


        public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            var expression = VisitSyntaxNode(node.Expression);
            _currentNode = new ParenthesizedExpression(expression);
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            var operand = VisitSyntaxNode(node.Operand);
            var @operator = node.OperatorToken.Text;
            _currentNode = new PostfixUnaryExpression(operand, @operator);
        }

        public override void VisitPredefinedType(PredefinedTypeSyntax node)
        {
            _currentNode = new TypeExpression(node.Keyword.Text); 
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var operand = VisitSyntaxNode(node.Operand);
            var @operator = node.OperatorToken.Text;
            _currentNode = new PrefixUnaryExpression(operand, @operator);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var type = node.Type.ToString();
            var name = node.Identifier.ValueText;
            var modifiers = node.Modifiers.Select(m => m.ValueText);

            _currentNode = new PropertyDeclaration(type, name, null, modifiers);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            var expression = VisitSyntaxNode(node.Expression);
            var sections = new List<SwitchSection>();
            foreach(var section in node.Sections)
            {
                sections.Add(VisitSyntaxNode(section) as SwitchSection);
            }

            _currentNode = new SwitchStatement(expression, sections);
        }

        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            _currentNode = VisitSyntaxNode(node.Value);
        }

        public override void VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
        {
            _currentNode = new Literal("default");
        }

        public override void VisitSwitchSection(SwitchSectionSyntax node)
        {
            var labels = new List<Node>();
            foreach(var label in node.Labels)
            {
                var labelNode = VisitSyntaxNode(label);
                if (labelNode != null)
                    labels.Add(labelNode);
            }
            
            var statements = new List<Node>();
            foreach(var statement in node.Statements)
            {
                statements.Add(VisitSyntaxNode(statement));
            }

            _currentNode = new SwitchSection(labels, statements);
            
        }

        public override void VisitThisExpression(ThisExpressionSyntax node)
        {
            _currentNode = new ThisExpression();
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            var statement = VisitSyntaxNode(node.Expression);
            _currentNode = new Throw(statement);
        }

        public override void VisitTryStatement(TryStatementSyntax node)
        {
            var block = VisitSyntaxNode(node.Block) as Block;
            var catches = new List<Catch>();
            foreach(var @catch in node.Catches)
            {
                catches.Add(VisitSyntaxNode(@catch) as Catch);
            }

            var fin = VisitSyntaxNode(node.Finally) as Finally;

            _currentNode = new Try(block, catches, fin);
        }

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            Node declaration = null;
            if (node.Declaration != null)
            {
                declaration = VisitSyntaxNode(node.Declaration);
            }
            else if(node.Expression != null)
            {
                declaration = VisitSyntaxNode(node.Expression);
            }

            var expression = VisitSyntaxNode(node.Statement);
            _currentNode = new Using(expression, declaration);
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            var expression = VisitSyntaxNode(node.Expression);
            _currentNode = new ReturnStatement(expression);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var type = node.Type.ToString();
            if (node.Type.ToString() == "var")
            {
                type = null;
            }

            var variables = new List<VariableDeclarator>();
            foreach(var variable in node.Variables)
            {
                variables.Add(VisitSyntaxNode(variable) as VariableDeclarator);
            }

            _currentNode = new VariableDeclaration(type, variables);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var name = node.Identifier.ToString();
            var initializer = VisitSyntaxNode(node.Initializer);

            _currentNode = new VariableDeclarator(name, initializer);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            var condition = VisitSyntaxNode(node.Condition);
            var statement = VisitSyntaxNode(node.Statement);

			var block = statement as Block;
			if (block == null)
				block = new Block(statement);

            _currentNode = new While(condition, block);
        }

        public override void VisitIncompleteMember(IncompleteMemberSyntax node)
        {
	        throw new IncompleteCodeBlockException();
        }

    }
}
