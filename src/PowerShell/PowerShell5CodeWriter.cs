using CodeConverter.Common;
using CodeConverter.PowerShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellToolsPro.CodeConversion.PowerShell
{
    public class PowerShell5CodeWriter : PowerShellCodeWriter
    {
        public override Language Language => Language.PowerShell5;

        public override void VisitClassDeclaration(ClassDeclaration node)
        {
            Append($"class {node.Name}");

            if (node.Bases.Any())
            {
                Append(" : ");
                Append(node.Bases.Aggregate((x, y) => x + ", " + y));
            }

            NewLine();

            Append("{");
            Indent();
            foreach (var member in node.Members)
            {
                NewLine();
                member.Accept(this);
            }
            NewLine();
            Outdent();
            Append("}");
        }

        public override void VisitMethodDeclaration(MethodDeclaration node)
        {
            var staticMod = string.Empty;
            if (node.Modifiers.Contains("static"))
            {
                staticMod = "static ";
            }

            Append($"{staticMod}[{node.ReturnType}] {node.Name}(");

            int index = 0;
            foreach(var parameter in node.Parameters)
            {
                parameter.Accept(this);
                if (index < node.Parameters.Count() - 1)
                {
                    Append(",");
                }
            }
            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();
            node.Body.Accept(this);
            Outdent();
            Append("}");
        }

        public override void VisitPropertyDeclaration(PropertyDeclaration node)
        {
            var hidden = string.Empty;
            if (node.Modifiers.Contains("private"))
            {
                hidden = "hidden ";
            }

            var staticMod = string.Empty;
            if (node.Modifiers.Contains("static"))
            {
                staticMod = "static ";
            }

            Append($"{staticMod}[{node.Type}] {hidden}${node.Name}");
        }

        public override void VisitFieldDeclaration(FieldDeclaration node)
        {
            var hidden = string.Empty;
            if (node.Modifiers.Contains("private"))
            {
                hidden = "hidden ";
            }

            var staticMod = string.Empty;
            if (node.Modifiers.Contains("static"))
            {
                staticMod = "static ";
            }

            Append($"{staticMod}[{node.Type}] {hidden}${node.Name}");
        }

        public override void VisitThisExpression(ThisExpression node)
        {
            Append("$this");
        }

        public override void VisitConstructor(Constructor node)
        {
            Append($"{node.Identifier} (");
            int index = 0;
            foreach (var parameter in node.ArgumentList.Arguments)
            {
                parameter.Accept(this);
                if (index < node.ArgumentList.Arguments.Count() - 1)
                {
                    Append(",");
                }
            }
            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();
            node.Body.Accept(this);
            Outdent();
            Append("}");
        }

    }
}
