using System.Collections.Generic;
using System.Linq;

namespace CodeConverter.Common
{
    public abstract class CStyleCodeWriter : CodeWriter
    {
		private static Dictionary<BinaryOperator, string> _operatorMap;

		protected bool TerminateStatementWithSemiColon { get; set; }
		protected virtual Dictionary<BinaryOperator, string> OperatorMap => _operatorMap;

		static CStyleCodeWriter()
		{
			_operatorMap = new Dictionary<BinaryOperator, string>
			{
				{ BinaryOperator.Equal, " == " },
				{ BinaryOperator.NotEqual, " != " },
				{ BinaryOperator.GreaterThan, " > " },
				{ BinaryOperator.LessThan, " < " },
				{ BinaryOperator.LessThanEqualTo, " <= " },
				{ BinaryOperator.GreaterThanEqualTo, " >= " },
				{ BinaryOperator.And, " && " },
				{ BinaryOperator.Or, " || " },
				{ BinaryOperator.Bor, " | " },
				{ BinaryOperator.Minus, " - " },
				{ BinaryOperator.Plus, " + " },
				{ BinaryOperator.Not, " ! " }
			};
		}

		public override void VisitAssignment(Assignment node)
        {
            node.Left.Accept(this);
            Append(" = ");
            node.Right.Accept(this);
        }

        public override void VisitArgument(Argument node)
        {
            node?.Expression?.Accept(this);
        }

        public override void VisitArgumentList(ArgumentList node)
        {
            foreach (var argument in node.Arguments)
            {
                argument.Accept(this);

                Append(",");
            }

            //Remove trailing comma
            Builder.Remove(Builder.Length - 1, 1);
        }

        public override void VisitBinaryExpression(BinaryExpression node)
        {
            node.Left.Accept(this);

            if (OperatorMap.ContainsKey(node.Operator))
            {
                Append(OperatorMap[node.Operator]);
            }

            node.Right.Accept(this);
        }

        public override void VisitBlock(Block node)
        {
            if (node == null)
            {
                return;
            }

            foreach (var statement in node.Statements)
            {
                if (statement == null)
                {
                    continue;
                }

                statement.Accept(this);

				if (TerminateStatementWithSemiColon && Builder.Length > 0 && Builder[Builder.Length - 1] != '}' && Builder[Builder.Length - 1] != ';')
				{
					Append(";");
				}

                NewLine();
            }
        }

        public override void VisitBreak(Break node)
        {
            Append("break");
        }

        public override void VisitCast(Cast node)
        {
            Append("(");
            Append(node.Type);
            Append(")");
            node.Expression.Accept(this);
        }

        public override void VisitCatch(Catch node)
        {
            Append("catch");
            if (node.Declaration != null)
            {
                Append(" ");
                node.Declaration.Accept(this);
            }
            NewLine();
            Append("{");
            Indent();
            NewLine();

            node.Block.Accept(this);

            Outdent();
            Append("}");
        }

        public override void VisitCatchDeclaration(CatchDeclaration node)
        {
            Append("(");
            Append(node.Type);
            Append(")");
        }

        public override void VisitContinue(Continue node)
        {
            Append("continue");
        }

		public override void VisitBracketedArgumentList(BracketedArgumentList node)
		{
			Append("[");

			foreach (var argument in node.Arguments)
			{
				argument.Accept(this);

				Append(",");
			}

			//Remove trailing comma
			Builder.Remove(Builder.Length - 1, 1);

			Append("]");
		}

		public override void VisitElseClause(ElseClause node)
        {
            NewLine();
            Append("else");

            var isIf = node.Body is IfStatement;
            if (!isIf)
            {
                NewLine();
                Append("{");
                Indent();
                NewLine();
            }
			else
			{
				Append(" ");
			}

            node.Body.Accept(this);

            if (!isIf)
            {
                Outdent();
                Append("}");
            }
        }

        public override void VisitFinally(Finally node)
        {
            Append("finally");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            node.Body.Accept(this);

            Outdent();
            Append("}");
        }

        public override void VisitForStatement(ForStatement node)
        {
            Append("for(");

            if (node.Declaration != null)
            {
                node.Declaration.Accept(this);
            }
            else
            {
                foreach (var initializer in node.Initializers)
                {
                    initializer.Accept(this);
                }
            }

			Append("; ");

            node.Condition.Accept(this);

            Append("; ");

            foreach (var incrementor in node.Incrementors)
            {
                incrementor.Accept(this);
            }

            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();

			if (node.Statement is Block)
			{
				node.Statement.Accept(this);
			}
			else
			{
				var block = new Block(node.Statement);
				block.Accept(this);
			}
            
            Outdent();
            Append("}");
        }

        public override void VisitForEachStatement(ForEachStatement node)
        {
            Append("foreach (");
            node.Identifier.Accept(this);
            Append(" in ");
            node.Expression.Accept(this);
            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();

			if (node.Statement is Block)
			{
				node.Statement.Accept(this);
			}
			else
			{
				var block = new Block(node.Statement);
				block.Accept(this);
			}
            
            Outdent();
            Append("}");
        }

        public override void VisitIdentifierName(IdentifierName node)
        {
            Append(node.Name);
        }

        public override void VisitIfStatement(IfStatement node)
        {
            Append("if (");
            node.Condition.Accept(this);
            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            node.Body.Accept(this);

            Outdent();
			Append("}");

            node.ElseClause?.Accept(this);
        }

        public override void VisitInvocation(Invocation node)
        {
            node.Expression.Accept(this);

            if (!node.Arguments.Arguments.Any())
            {
                Append("()");
            }
            else
            {
                Append("(");
                node.Arguments.Accept(this);
                Append(")");
            }
        }

        public override void VisitLiteral(Literal node)
        {
            Append(node.Token);
        }

        public override void VisitMemberAccess(MemberAccess node)
        {
            if (node == null || node.Expression == null)
            {
                return;
            }

            node.Expression.Accept(this);
            Append(".");
            Append(node.Identifier);
        }

		public override void VisitParameter(Parameter node)
		{
			Append(node.Type);
			Append(" ");
			Append(node.Name);
		}

		public override void VisitParenthesizedExpression(ParenthesizedExpression node)
        {
            Append("(");
            node.Expression.Accept(this);
            Append(")");
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpression node)
        {
            node.Operand.Accept(this);
            Append("++");
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpression node)
        {
            Append("++");
            node.Operand.Accept(this);
        }

        public override void VisitStringConstant(StringConstant node)
        {
	        var escapedString = node.Value.Replace("\\", "\\\\");

            Append("\"" + escapedString + "\"");
        }

		public override void VisitSwitchStatement(SwitchStatement node)
		{
			Append("switch (");
			node.Expression.Accept(this);
			Append(")"); NewLine();
			Append("{"); Indent(); NewLine();

			foreach (var section in node.Sections)
			{
				section.Accept(this);
			}

			Outdent();
			Append("}");
		}

		public override void VisitSwitchSection(SwitchSection node)
		{
			foreach (var label in node.Labels)
			{
				var idName = label as IdentifierName;
				if (idName?.Name.Equals("default") == true)
				{
					Append("default:");
				}
				else
				{
					Append("case ");
					label.Accept(this);
					Append(":");
				}
				
				Indent();
				foreach (var statement in node.Statements)
				{
					NewLine();
					statement.Accept(this);
				}
				Outdent();
			}
		}

		public override void VisitTemplateStringConstant(TemplateStringConstant node)
        {
            Append("\"" + node.Value + "\"");
        }

        public override void VisitThrow(Throw node)
        {
            Append("throw ");
            node.Statement.Accept(this);
        }

        public override void VisitTry(Try node)
        {
            Append("try");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            node.Block.Accept(this);

            Outdent();
            Append("}");
            foreach (var @catch in node.Catches)
            {
                NewLine();
                @catch.Accept(this);
            }

            if (node.Finally != null)
            {
                NewLine();
                node.Finally.Accept(this);
            }
        }

		public override void VisitTypeExpression(TypeExpression node)
		{
			Append(node.TypeName);
		}

		public override void VisitUnknown(Unknown unknown)
        {
            Append(unknown.Message);
        }

	    public override void VisitRawCode(RawCode node)
	    {
		    Append(node.Code);
	    }

        public override void VisitReturnStatement(ReturnStatement node)
        {
            Append("return");

            if (node.Expression != null)
            {
                Append(" ");
                node.Expression.Accept(this);
            }

        }

        public override void VisitWhile(While node)
        {
            Append("while (");
            node.Condition.Accept(this);
            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();
            node.Statement.Accept(this);
            Outdent();
            Append("}");
        }

		public override void VisitVariableDeclaration(VariableDeclaration node)
		{
			if (!string.IsNullOrEmpty(node.Type))
			{
				Append(node.Type);
				Append(" ");
			}

			foreach (var variable in node.Variables)
			{
				VisitVariableDeclarator(variable);
			}
		}

		public override void VisitVariableDeclarator(VariableDeclarator node)
		{
			Append(node.Name);
			if (node.Initializer != null)
			{
				Append(" = ");
				node.Initializer.Accept(this);
			}
		}
	}
}
