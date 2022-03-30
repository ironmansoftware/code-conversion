using CodeConverter.Common;
using System.Collections.Generic;
using System.Linq;

namespace CodeConverter.PowerShell
{
    public class PowerShellCodeWriter : CStyleCodeWriter
    {
        private bool _inSwitch;
        private static Dictionary<BinaryOperator, string> _operatorMap;
        protected override Dictionary<BinaryOperator, string> OperatorMap => _operatorMap;

        static PowerShellCodeWriter()
        {
            _operatorMap = new Dictionary<BinaryOperator, string>
            {
                { BinaryOperator.Equal, " -eq " },
                { BinaryOperator.NotEqual, " -ne " },
                { BinaryOperator.GreaterThan, " -gt " },
                { BinaryOperator.LessThan, " -lt " },
                { BinaryOperator.LessThanEqualTo, " -le " },
                { BinaryOperator.GreaterThanEqualTo, " -ge " },
                { BinaryOperator.And, " -and " },
                { BinaryOperator.Or, " -or " },
                { BinaryOperator.Bor, " -bor " },
                { BinaryOperator.Minus, " - " },
                { BinaryOperator.Plus, " + " },
                { BinaryOperator.Not, " -not " }
            };
        }

        public override Language Language => Language.PowerShell;

        public override void VisitArrayCreation(ArrayCreation node)
        {
            Append("@(");
            foreach(var item in node.Initializer)
            {
                item.Accept(this);
                Append(",");
            }

            // Remove last ,
            Builder.Remove(Builder.Length - 1, 1);
            Append(")");
        }

        public override void VisitBreak(Break node)
        {
            if (_inSwitch) return;

            base.VisitBreak(node);
        }

        public override void VisitCast(Cast node)
        {
            Append("[");
            Append(node.Type);
            Append("]");
            node.Expression.Accept(this);
        }

        public override void VisitCatchDeclaration(CatchDeclaration node)
        {
            Append("[");
            Append(node.Type);
            Append("]");
        }

        public override void VisitIdentifierName(IdentifierName node)
        {
            Append("$");
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

			node.Body.Accept(this);

			if (!isIf)
			{
				Outdent();
				Append("}");
			}
		}

		public override void VisitLiteral(Literal node)
        {
            if (node.Token == "true" || node.Token == "false")
                Append("$");

            Append(node.Token);
        }

        public override void VisitMemberAccess(MemberAccess node)
        {
            if (node == null || node.Expression == null)
            {
                return;
            }

            var typeExpression = node.Expression as TypeExpression;
            if (typeExpression != null)
            {
                Append("[");
                Append(typeExpression.TypeName);
                Append("]::");
                Append(node.Identifier);
            }
            else
            {
                base.VisitMemberAccess(node);
            }
        }

        public override void VisitMethodDeclaration(MethodDeclaration node)
        {
            Append("function ");
            Append(node.Name);
            NewLine();
            Append("{");
            Indent();
            NewLine();

            if (node.Parameters.Any())
            {
                Append("param(");
                foreach (var parameter in node.Parameters)
                {
                    parameter.Accept(this);
                    Append(", ");
                }
                Builder.Remove(Builder.Length - 2, 2);
                Append(")");
                NewLine();
            }

            if (node.Modifiers.Contains("extern") && node.Attributes.Any(m => m.Name == "DllImport"))
            {
                Append("Add-Type -TypeDefinition '"); Indent(); NewLine();
                Append("using System;"); NewLine();
                Append("using System.Runtime.InteropServices;"); NewLine();
                Append("public static class PInvoke {"); Indent(); NewLine();

                foreach(var line in node.OriginalSource.Split('\r'))
                {
                    var trimmedLine = line.Trim();
                    Append(trimmedLine);
                    NewLine();
                }

                Outdent();
                Append("}"); NewLine(); Outdent();
                Append("'");
                NewLine(); 
                Append("[PInvoke]::"); Append(node.Name); Append("(");

                foreach (var parameter in node.Parameters)
                {
                    if (parameter.Modifiers.Any(m => m == "ref" || m == "out"))
                    {
                        Append("[ref]");
                    }
                    Append("$"); Append(parameter.Name); Append(", ");
                }

                Builder.Remove(Builder.Length - 2, 2);
                Append(")"); NewLine();
            }
            else
            {
                node.Body?.Accept(this);
            }

            Outdent();
            Append("}");
            NewLine();
        }

        public override void VisitObjectCreation(ObjectCreation node)
        {
            var typeName = node.Type;

            Append("(New-Object -TypeName ");
            Append(typeName);

            if (!node.Arguments.Arguments.Any())
            {
                Append(")");
                return;
            };

            Append(" -ArgumentList ");

            VisitArgumentList(node.Arguments);

            Append(")");
        }

        public override void VisitParameter(Parameter node)
        {
            if (node.Modifiers.Any(m => m == "ref" || m == "out"))
            {
                Append("[ref]");
            }

            if (!string.IsNullOrEmpty(node.Type))
            {
                Append("[");
                Append(node.Type);
                Append("]");
            }

            Append("$");
            Append(node.Name);
        }

        public override void VisitStringConstant(StringConstant node)
        {
            Append("\'" + node.Value + "\'");
        }

        public override void VisitSwitchStatement(SwitchStatement node)
        {
            _inSwitch = true;
            Append("switch (");
            node.Expression.Accept(this);
            Append(")"); NewLine();
            Append("{"); Indent(); NewLine();

            foreach(var section in node.Sections)
            {
                section.Accept(this);
            }

            Outdent();
            Append("}");
            _inSwitch = false;
        }

        public override void VisitSwitchSection(SwitchSection node)
        {
            foreach(var label in node.Labels)
            {
                label.Accept(this);
                Append(" {"); Indent(); NewLine();
                foreach(var statement in node.Statements)
                {
                    statement.Accept(this);
                    NewLine();
                }
                Outdent();
                Append("}");
                NewLine();
            }
        }

        public override void VisitUsing(Using node)
        {
            string variableName = null;
            Node initializer = null;
            var variableDeclaration = node.Declaration as VariableDeclaration;
            if (variableDeclaration != null && variableDeclaration.Variables.Any())
            {
                var variableDeclartor = variableDeclaration.Variables.First();
                if (!string.IsNullOrEmpty(variableDeclaration.Type))
                {
                    Append("[");
                    Append(variableDeclaration.Type);
                    Append("]");
                }

                variableName = variableDeclartor.Name;
                initializer = variableDeclartor.Initializer;

                Append("$");
                Append(variableDeclartor.Name);
                Append(" = $null");
                NewLine();
            }

            var identifierName = node.Declaration as IdentifierName;
            if (identifierName != null)
            {
                variableName = identifierName.Name;
            }

            Append("try");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            if (initializer != null)
            {
                Append("$");
                Append(variableName);
                Append(" = ");
                initializer.Accept(this);
                NewLine();
            }

            node.Expression.Accept(this);
            Outdent();
            Append("}");
            NewLine();
            Append("finally");
            NewLine();
            Append("{");
            Indent();
            NewLine();

            if (variableName != null)
            {
                Append("$");
                Append(variableName);
                Append(".Dispose()");
                NewLine();
            }

            Outdent();
            Append("}");
        }

        public override void VisitVariableDeclaration(VariableDeclaration node)
        {
            if (!string.IsNullOrEmpty(node.Type))
            {
                Append("[");
                Append(node.Type);
                Append("]");
            }
            
            foreach(var variable in node.Variables)
            {
                VisitVariableDeclarator(variable);
            }
        }

        public override void VisitVariableDeclarator(VariableDeclarator node)
        {
            Append("$");
            Append(node.Name);
            if (node.Initializer != null)
            {
                Append(" = ");
                node.Initializer.Accept(this);
            }
        }
    }
}
