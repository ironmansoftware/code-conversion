using CodeConverter.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace CodeConverter.PowerShell
{
    public class PowerShellSyntaxTreeVisitor : ISyntaxTreeVisitor
    {
        public Language Language => Language.PowerShell;

        public Node Visit(string code)
        {
	        Token[] tokens;
	        ParseError[] errors;
            var ast = Parser.ParseInput(code, out tokens, out errors);
            var visitor = new PowerShellAstVisitor();
            ast.Visit(visitor);
            return visitor.Node;
        }
    }

    internal class PowerShellAstVisitor : AstVisitor
    {
        private static Dictionary<TokenKind, BinaryOperator> _operatorMap;
		private List<string> _functionDefinitions = new List<string>();

        static PowerShellAstVisitor()
        {
            _operatorMap = new Dictionary<TokenKind, BinaryOperator>
            {
                { TokenKind.And, BinaryOperator.And },
                { TokenKind.Ieq, BinaryOperator.Equal },
				{ TokenKind.Ine, BinaryOperator.NotEqual },
				{ TokenKind.Not, BinaryOperator.Not},
				{ TokenKind.Ilt, BinaryOperator.LessThan },
				{ TokenKind.Or, BinaryOperator.Or },
				{ TokenKind.Igt, BinaryOperator.GreaterThan },
				{ TokenKind.Ige, BinaryOperator.GreaterThanEqualTo },
				{ TokenKind.Ile, BinaryOperator.LessThanEqualTo },
				{ TokenKind.Plus, BinaryOperator.Plus },
				{ TokenKind.Minus, BinaryOperator.Minus },
				{ TokenKind.Bor, BinaryOperator.Bor }
			};
        }

        private Node _currentNode;
        public Node Node => _currentNode;

        public Node VisitSyntaxNode(Ast node)
        {
            node?.Visit(this);
            return _currentNode;
        }
		public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
		{
			var elements = new List<Node>();
			foreach(var element in arrayLiteralAst.Elements)
			{
				elements.Add(VisitSyntaxNode(element));
			}

			_currentNode = new ArrayCreation(elements, string.Empty);

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
			if (assignmentStatementAst.Left is ConvertExpressionAst)
			{
				var varExpression = assignmentStatementAst.Left as ConvertExpressionAst;
				if (varExpression.Attribute != null && varExpression.Child is VariableExpressionAst)
				{
					var typeName = varExpression.Attribute.TypeName.Name;
					var name = (varExpression.Child as VariableExpressionAst).VariablePath.ToString();
					var initializer = VisitSyntaxNode(assignmentStatementAst.Right);
					_currentNode = new VariableDeclaration(typeName, new VariableDeclarator(name, initializer));

					return AstVisitAction.SkipChildren;
				}
			}

			var left = VisitSyntaxNode(assignmentStatementAst.Left);
			var right = VisitSyntaxNode(assignmentStatementAst.Right);
            _currentNode = new Assignment(left, right);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            var left = VisitSyntaxNode(binaryExpressionAst.Left);
            var @operator = BinaryOperator.Unknown;

            if (_operatorMap.ContainsKey(binaryExpressionAst.Operator))
            {
                @operator = _operatorMap[binaryExpressionAst.Operator];
            }

            var right = VisitSyntaxNode(binaryExpressionAst.Right);

            _currentNode = new BinaryExpression(left, @operator, right);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            _currentNode = VisitSyntaxNode(blockStatementAst.Body);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            _currentNode = new Break();

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            var body = VisitSyntaxNode(catchClauseAst.Body);
			var block = body as Block;
			if (block == null)
			{
				block = new Block(body);
			}

            var type = catchClauseAst.CatchTypes.FirstOrDefault();
            CatchDeclaration declaration = null;
            if (type != null)
            {
				var myType = type.TypeName.Name;
                declaration = new CatchDeclaration(myType.ToString(), null);
            }

            _currentNode = new Catch(declaration, block);

            return AstVisitAction.SkipChildren;
        }

		public override AstVisitAction VisitCommand(CommandAst commandAst)
		{
			var name = commandAst.GetCommandName();
			var elements = commandAst.CommandElements.Skip(1).ToArray();
			var arguments = new List<Argument>();

			var parameters = ParameterFinder.FindBoundParameters(commandAst);
			if (parameters != null)
			{
				foreach (var parameter in parameters)
				{
					if (parameter.Value is Ast)
					{
						var ast = parameter.Value as Ast;
						var node = VisitSyntaxNode(ast);
						var argument = new Argument(parameter.Key, node);
						arguments.Add(argument);
					}
					else if (parameter.Value is ExpressionAst[])
					{
						var ast = (parameter.Value as ExpressionAst[]).First();
						var node = VisitSyntaxNode(ast);
						var argument = new Argument(parameter.Key, node);
						arguments.Add(argument);
					}
					// This is happening because we are getting something from the pipeline
					else if (parameter.Value == null)
					{
						var literal = new Literal("");//TODO: Need to represent null
						var argument = new Argument(parameter.Key, literal);
						arguments.Add(argument);
					}
					else
					{
						var literal = new Literal(parameter.Value.ToString());
						var argument = new Argument(parameter.Key, literal);
						arguments.Add(argument);
					}
				}
			}
			else
            {
	            for (var i = 0; i < elements.Length; i++)
	            {
		            var node = VisitSyntaxNode(elements[i]);
		            var argument = node as Argument;
		            if (argument != null && i < elements.Length - 1 && argument.Expression == null)
		            {
			            node = VisitSyntaxNode(elements[i + 1]);
			            argument.Expression = node;
			            i++;
		            }
		            else if (argument == null)
		            {
			            argument = new Argument(node);
		            }
		            arguments.Add(argument);
	            }
            }

			_currentNode = ConvertCommand(new Invocation(new IdentifierName(name), new ArgumentList(arguments)));
			
			if (_currentNode.Intent == null && !name.Equals("New-Object", StringComparison.OrdinalIgnoreCase) &&  !_functionDefinitions.Any(m => m.Equals(name, StringComparison.OrdinalIgnoreCase)))
			{
				var psAssignment = new VariableDeclaration("System.Management.Automation.PowerShell", new VariableDeclarator("powershell", new Invocation(new MemberAccess(new TypeExpression("System.Management.Automation.PowerShell"), "Create"), new ArgumentList())));
				var addCommand = new Invocation(new MemberAccess(new IdentifierName("powershell"), "AddCommand"), new ArgumentList(new StringConstant(name)));

				var statements = new List<Node> {addCommand};
				foreach (var argument in arguments)
				{
					statements.Add(new Invocation(new MemberAccess(new IdentifierName("powershell"), "AddArgument"), new ArgumentList(argument)));
				}

				var invokeCommand = new Invocation(new MemberAccess(new IdentifierName("powershell"), "Invoke"), new ArgumentList());
				statements.Add(invokeCommand);

				_currentNode = new Using(new Block(statements), psAssignment);
			}

			return AstVisitAction.SkipChildren;
		}

		public Node ConvertCommand(Invocation node)
		{
			var commandIntentFactory = new CommandIntentFactory();
			node.Intent = commandIntentFactory.DetermineCommandIntent(node);

			var name = node.Expression as IdentifierName;
			if (name == null)
				return node;

			if (name.Name.Equals("New-Object", StringComparison.OrdinalIgnoreCase))
			{
				var typeNameArg = node.Arguments.Arguments.Cast<Argument>().FirstOrDefault(m => m.Name.Equals("TypeName", StringComparison.OrdinalIgnoreCase));
				var argumentListArgs = node.Arguments.Arguments.Cast<Argument>().FirstOrDefault(m => m.Name.Equals("ArgumentList", StringComparison.OrdinalIgnoreCase));

				var typeName = typeNameArg?.Expression as StringConstant;
				if (typeName == null)
					return node;

				var argList = argumentListArgs?.Expression as ArgumentList;
				if (argumentListArgs != null && argList == null)
				{
					argList = new ArgumentList(argumentListArgs.Expression);
				}

				//Unwind an array. We likely dont want this array
				if (argList != null && argList.Arguments.Count() == 1 && argList.Arguments.All(m => m is ArrayCreation))
				{
					var array = argList.Arguments.Cast<ArrayCreation>().First();
					argList = new ArgumentList(array.Initializer);
				}


				return new ObjectCreation(typeName.Value, argList);
			}

			return node;
		}

		public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
		{
			Node expression = null;
			if (commandParameterAst.Argument != null)
				expression = VisitSyntaxNode(commandParameterAst.Argument);

			_currentNode = new Argument(commandParameterAst.ParameterName, expression);

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            _currentNode = new Literal(constantExpressionAst.Value.ToString());
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            _currentNode = new Continue();
            return AstVisitAction.SkipChildren;
        }

		public override AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
		{
			var expression = VisitSyntaxNode(convertExpressionAst.Child);
			var type = convertExpressionAst.Type.TypeName.Name;

			_currentNode = new Cast(type, expression);

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitForStatement(ForStatementAst forStatementAst)
		{
			var body = VisitSyntaxNode(forStatementAst.Body);
			var condition = VisitSyntaxNode(forStatementAst.Condition);
			var initializer = VisitSyntaxNode(forStatementAst.Initializer);
			var iterator = VisitSyntaxNode(forStatementAst.Iterator);

			_currentNode = new ForStatement(initializer, iterator, condition, body);

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            var body = VisitSyntaxNode(forEachStatementAst.Body);
            var condition = VisitSyntaxNode(forEachStatementAst.Condition);
            var variable = VisitSyntaxNode(forEachStatementAst.Variable);

			_currentNode = new ForEachStatement(variable as IdentifierName, condition, body);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            var body = VisitSyntaxNode(functionDefinitionAst.Body);
            var name = functionDefinitionAst.Name;
            var parameters = new List<Parameter>();

	        _functionDefinitions.Add(name);

			if (functionDefinitionAst.Parameters != null)
            {
                foreach (var parameter in functionDefinitionAst.Parameters)
                {
                    parameters.Add(new Parameter(parameter.StaticType?.ToString(), parameter.Name.ToString()));
                }
            }

			if (functionDefinitionAst.Body.ParamBlock != null)
			{
				foreach (var parameter in functionDefinitionAst.Body.ParamBlock.Parameters)
				{
					parameters.Add(new Parameter(parameter.StaticType?.Name, parameter.Name.ToString().TrimStart('$')));
				}
			}
           
            _currentNode = new MethodDeclaration(name, parameters, body, new[] { "public" }, new CodeConverter.Common.Attribute[0], "void");

            return AstVisitAction.SkipChildren;
        }

	    public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
	    {
		    var members = new List<AnonymousTypeMember>();

		    foreach (var kvp in hashtableAst.KeyValuePairs)
		    {
			    var anonymousTypeMember = new AnonymousTypeMember(kvp.Item1.ToString(), VisitSyntaxNode(kvp.Item2));
			    members.Add(anonymousTypeMember);
		    }

			_currentNode = new AnonymousType(members);

		    return AstVisitAction.SkipChildren;
		}

	    public override AstVisitAction VisitIfStatement(IfStatementAst ifStmtAst)
        {
            var firstCondition = ifStmtAst.Clauses.First();
            var condition = VisitSyntaxNode(firstCondition.Item1);
            var body = VisitSyntaxNode(firstCondition.Item2);

			body = EnsureBlock(body);

			// If
			var ifStatement = new IfStatement(condition, body);

            var previousIf = ifStatement;

            // Else ifs
            foreach (var clause in ifStmtAst.Clauses.Skip(1))
            {
				condition = VisitSyntaxNode(clause.Item1);
                body = VisitSyntaxNode(clause.Item2);
				body = EnsureBlock(body);
				var nextCondition = new IfStatement(condition, body);
                previousIf.ElseClause = new ElseClause(nextCondition);
                previousIf = nextCondition;
            }

            // Else
            if (ifStmtAst.ElseClause != null)
            {
                var statements = new List<Node>();
                foreach(var statement in ifStmtAst.ElseClause.Statements)
                {
                    var statementNode = VisitSyntaxNode(statement);
                    statements.Add(statementNode);
                }
                body = new Block(statements);
                previousIf.ElseClause = new ElseClause(body);
            }

            _currentNode = ifStatement;

            return AstVisitAction.SkipChildren;
        }

		public override AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst)
		{
			var target = VisitSyntaxNode(indexExpressionAst.Target);
			var index = VisitSyntaxNode(indexExpressionAst.Index);

			_currentNode = new ElementAccess(target, new BracketedArgumentList(new ArgumentList(index)));

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            var arguments = new List<Argument>();

			if (methodCallAst.Arguments != null)
			{
				foreach (var argument in methodCallAst.Arguments)
				{
					var arg = VisitSyntaxNode(argument);
					var ar = new Argument(arg);
					arguments.Add(ar);
				}
			}
           

            var expression = VisitSyntaxNode(methodCallAst.Expression);

            var methodName = methodCallAst.Member.ToString();

			var memberAccess = new MemberAccess(expression, methodName);

            var argumentList = new ArgumentList(arguments);

            _currentNode = new Invocation(memberAccess, argumentList);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitParameter(ParameterAst parameterAst)
        {
            var name = parameterAst.Name.ToString();
            var type = parameterAst.StaticType?.Name;
            _currentNode = new Parameter(type, name);

            return AstVisitAction.SkipChildren;
        }

		public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
		{
			var expression = VisitSyntaxNode(memberExpressionAst.Expression);
			var member = VisitSyntaxNode(memberExpressionAst.Member);

			if (member is StringConstant)
			{
				var str = member as StringConstant;
				_currentNode = new MemberAccess(expression, str.Value);
			}
			
			return AstVisitAction.SkipChildren; 
		}

		public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
		{
			var name = typeExpressionAst.TypeName.Name;

			_currentNode = new TypeExpression(name);

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            _currentNode = new StringConstant(stringConstantExpressionAst.Value);

            return AstVisitAction.SkipChildren;
        }

		

        public override AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            _currentNode = new TemplateStringConstant(expandableStringExpressionAst.Value);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
			var node = VisitSyntaxNode(returnStatementAst.Pipeline);
            _currentNode = new ReturnStatement(node);
            return AstVisitAction.SkipChildren;
        }

		public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
		{
			var statements = new List<Node>();

			if (scriptBlockAst.BeginBlock != null)
				foreach (var statement in scriptBlockAst.BeginBlock.Statements)
				{
					statements.Add(VisitSyntaxNode(statement));
				}

			if (scriptBlockAst.ProcessBlock != null)
				foreach (var statement in scriptBlockAst.ProcessBlock.Statements)
				{
					statements.Add(VisitSyntaxNode(statement));
				}

			if (scriptBlockAst.EndBlock != null)
				foreach (var statement in scriptBlockAst.EndBlock.Statements)
				{
					statements.Add(VisitSyntaxNode(statement));
				}

			_currentNode = new Block(statements);

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            var statements = new List<Node>();
            foreach(var statement in statementBlockAst.Statements)
            {
                statements.Add(VisitSyntaxNode(statement));
            }

			if (statements.Count() == 1)
			{
				_currentNode = statements.FirstOrDefault();
			}
			else
			{
				_currentNode = new Block(statements);
			}
            

            return AstVisitAction.SkipChildren;
        }

		public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
		{
			var condition = VisitSyntaxNode(switchStatementAst.Condition);

			var sections = new List<SwitchSection>();

			foreach(var clause in switchStatementAst.Clauses)
			{
				var cond = VisitSyntaxNode(clause.Item1);
				var body = VisitSyntaxNode(clause.Item2);

				var block = new Block(body, new Break());

				var section = new SwitchSection(new[] { cond }, new[] { block });

				sections.Add(section);
			}

			if (switchStatementAst.Default != null)
			{
				var body = VisitSyntaxNode(switchStatementAst.Default) as Block;

				var statements = body.Statements.ToList();
				statements.Add(new Break());

				var section = new SwitchSection(new[] { new IdentifierName("default") }, new[] { new Block(statements) });
				sections.Add(section);
			}
			

			_currentNode = new SwitchStatement(condition, sections);

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst)
        {
            var tryBody = VisitSyntaxNode(tryStatementAst.Body);
			var body = tryBody as Block;
			if (body == null)
			{
				body = new Block(tryBody);
			}

            var catches = new List<Catch>();
            foreach(var catchClause in  tryStatementAst.CatchClauses)
            {
                var catchNode = VisitSyntaxNode(catchClause) as Catch;
                catches.Add(catchNode);
            }

			if (tryStatementAst.Finally != null)
			{
				var fin = VisitSyntaxNode(tryStatementAst.Finally);
				var finBody = fin as Block;
				if (finBody == null)
				{
					finBody = new Block(fin);
				}

				var fina = new Finally(finBody);

				_currentNode = new Try(body, catches, fina);
			}
            else
			{
				_currentNode = new Try(body, catches);
			}

			

            

            return AstVisitAction.SkipChildren;
        }

		public override AstVisitAction VisitThrowStatement(ThrowStatementAst throwStatementAst)
		{
			var expression = VisitSyntaxNode(throwStatementAst.Pipeline);

			_currentNode = new Throw(expression);

			return AstVisitAction.SkipChildren;
		}

		public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            var child = VisitSyntaxNode(unaryExpressionAst.Child);
            if (unaryExpressionAst.TokenKind == TokenKind.PostfixPlusPlus)
            {
                _currentNode = new PostfixUnaryExpression(child, "++");
            }
            else if (unaryExpressionAst.TokenKind == TokenKind.PlusPlus)
            {
                _currentNode = new PrefixUnaryExpression(child, "++");
            }
            else if (unaryExpressionAst.TokenKind == TokenKind.PostfixMinusMinus)
            {
                _currentNode = new PostfixUnaryExpression(child, "--");
            }
            else if (unaryExpressionAst.TokenKind == TokenKind.MinusMinus)
            {
                _currentNode = new PrefixUnaryExpression(child, "--");
            }

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            var body = VisitSyntaxNode(whileStatementAst.Body);
            var condition = VisitSyntaxNode(whileStatementAst.Condition);

			var block = body as Block;
			if (block == null)
			{
				block = new Block(body);
			}

            _currentNode = new While(condition, block);

            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            var expression = VisitSyntaxNode(parenExpressionAst.Pipeline);
            _currentNode = new ParenthesizedExpression(expression);
            return AstVisitAction.SkipChildren;
        }

	    public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
	    {
		    if (pipelineAst.PipelineElements.Count == 1)
		    {
			    var pipelineElement = pipelineAst.PipelineElements[0];
			    _currentNode = VisitSyntaxNode(pipelineElement);

				return AstVisitAction.SkipChildren;
		    }

		    var elements = new List<Node>();

		    foreach (var element in pipelineAst.PipelineElements)
		    {
			    var node = VisitSyntaxNode(element);
			    elements.Add(node);
		    }

			_currentNode = new PipelineExpression(elements);

			return AstVisitAction.SkipChildren;
	    }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            var variableName = variableExpressionAst.VariablePath.ToString();
            _currentNode = new IdentifierName(variableName);
            return AstVisitAction.SkipChildren;
        }

		private Node EnsureBlock(Node node)
		{
			var block = node as Block;
			if (block == null)
			{
				return new Block(node);
			}
			return node;
		}
	}

}
