using UnityEditor.Experimental.GraphView;

public interface IAgentGraphNode
{
    public void Draw();
}

public abstract class AgentGraphNode : Node, IAgentGraphNode
{
    public abstract void Draw();
}
