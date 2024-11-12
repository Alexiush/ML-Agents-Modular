public class NodePathAttribute : System.Attribute
{
    public string Path;

    public NodePathAttribute(string path) 
    { 
        Path = path;
    }
}
