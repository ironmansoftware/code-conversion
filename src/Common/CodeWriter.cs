using System;
using System.Text;

namespace CodeConverter.Common
{
    /// <summary>
    /// Writes code based on a abstract syntax tree.
    /// </summary>
    public abstract class CodeWriter : NodeVisitor
    {
		protected IntentVisitor IntentVisitor { get; set; }
        /// <summary>
        /// Builder containing the code to write.
        /// </summary>
        protected StringBuilder Builder { get; private set; }
        /// <summary>
        /// The current indentation depth.
        /// </summary>
        protected int IndentationDepth { get; private set; }
        /// <summary>
        /// The character used for indentation. Usually spaces vs tabs.
        /// </summary>
        public string IndentationCharacter { get; set; } = "\t";

        /// <summary>
        /// Writes an abstract syntax tree to a string.
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public string Write(Node ast)
        {
            Builder = new StringBuilder();
            ast.Accept(this);
            return Builder.ToString();
        }

        /// <summary>
        /// The language supported by this code writer.
        /// </summary>
        public abstract Language Language { get; }

        /// <summary>
        /// Appends a string to the output code.
        /// </summary>
        /// <param name="str"></param>
        protected void Append(string str)
        {
            Builder.Append(str);
        }

        /// <summary>
        /// Appends a new line of text to the output code.
        /// </summary>
        /// <param name="str"></param>
        protected void AppendLine(string str)
        {
            Builder.AppendLine(str);
        }
       
        /// <summary>
        /// Inserts a new line character
        /// </summary>
        protected void NewLine()
        {
            Append(Environment.NewLine);
            for (int i = 0; i < IndentationDepth; i++)
                Append(IndentationCharacter);
        }
        /// <summary>
        /// Indents the code one level.
        /// </summary>
        protected void Indent()
        {
            IndentationDepth++;
        }
        /// <summary>
        /// Outdents the code one level.
        /// </summary>
        protected void Outdent()
        {
            Builder = Builder.Remove(Builder.Length - IndentationCharacter.Length, IndentationCharacter.Length);
            IndentationDepth--;
        }
    }
}
