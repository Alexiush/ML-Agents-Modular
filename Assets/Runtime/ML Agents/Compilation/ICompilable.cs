namespace ModularMLAgents.Compilation
{
    public class Expression
    {
        public string Name;
        public string Body;
    }

    public interface ICompilable
    {
        public Expression Compile(CompilationContext compilationContext);
    }
}
