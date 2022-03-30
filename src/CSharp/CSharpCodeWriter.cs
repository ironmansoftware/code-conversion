using CodeConverter.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeConverter.CSharp
{
    public class CSharpCodeWriter : CStyleCodeWriter
    {
        public override Language Language => Language.CSharp;

		public CSharpCodeWriter()
		{
			TerminateStatementWithSemiColon = true;
			IntentVisitor = new CSharpIntentVisitor();
		}

	    public override void VisitAnonymousType(AnonymousType node)
	    {
			Append("new {");
		    NewLine();
		    foreach (var member in node.Members)
		    {
			    Append(member.Name);
			    Append(" = "); 
				member.Value.Accept(this);
				Append(",");
			    NewLine();
		    }
			Append("}");
	    }

		public override void VisitArrayCreation(ArrayCreation node)
		{
			Append("new ");
			Append(node.Type);
			Append("[] { ");
			foreach(var element in node.Initializer)
			{
				element.Accept(this);

				Append(", ");
			}

			//Remove trailing comma
			Builder.Remove(Builder.Length - 2, 2);

			Append(" };");
		}

		public override void VisitInvocation(Invocation node)
		{
			if (IntentVisitor != null && node.Intent != null)
			{
				var alternateNode = node.Intent.Accept(IntentVisitor);
				alternateNode.Accept(this);
				return;
			}

			base.VisitInvocation(node);
		}

	    public override void VisitPipelineExpression(PipelineExpression node)
	    {
		    foreach (var element in node.Elements)
		    {
			    element.Accept(this);
			    Append(".");
		    }

		    Builder.Remove(Builder.Length - 1, 1);
		}

		public override void VisitObjectCreation(ObjectCreation node)
		{
			Append("new ");
			Append(node.Type);
			Append("(");

			node.Arguments?.Accept(this);

			Append(")");
		}

		public override void VisitForEachStatement(ForEachStatement node)
		{
			Append("foreach (var ");
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



		public override void VisitMethodDeclaration(MethodDeclaration node)
        {
            Append("void ");
            Append(node.Name);
            Append("(");

            if (node.Parameters.Any())
            {
                foreach (var parameter in node.Parameters)
                {
                    parameter.Accept(this);
                    Append(", ");
                }
                Builder.Remove(Builder.Length - 2, 2);
            }

            Append(")");
            NewLine();
            Append("{");
            Indent();
            NewLine();

			var block = node.Body as Block;
			if (block == null)
			{
				block = new Block(node.Body);
			}
			
			block.Accept(this);

			Outdent();
            Append("}");
        }

	    public override void VisitUsing(Using node)
	    {
		    Append("using (");
			node.Declaration.Accept(this);
			Append(")");
			NewLine();
			Append("{");
			Indent();
			NewLine();
			node.Expression.Accept(this);
			Outdent();
			Append("}");
	    }
    }
}
