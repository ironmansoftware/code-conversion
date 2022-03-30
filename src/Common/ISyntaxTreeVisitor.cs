namespace CodeConverter.Common
{
    public interface ISyntaxTreeVisitor
    {
        Node Visit(string code);
        Language Language { get; }
    }
}
