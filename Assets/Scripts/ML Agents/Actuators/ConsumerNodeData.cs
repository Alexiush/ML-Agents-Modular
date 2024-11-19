using System.Linq;
using UnityEngine;

[System.Serializable]
public class ConsumerNodeData : AgentGraphNodeData
{
    [SerializeField] public Consumer Consumer;

    public override AgentGraphNode Load()
    {
        var consumerNode = new ConsumerNode(Metadata);
        consumerNode.SetPosition(Metadata.Position);
        Ports.ForEach(p => p.Instantiate(consumerNode));

        return consumerNode;
    }

    public override string GetExpressionBody(CompilationContext compilationContext)
    {
        // For now: get inputs
        var inputs = compilationContext.GetInputs(this);
        // Consumer has the only input
        var input = inputs.First();

        compilationContext.RegisterEndpoint(this);

        // Consumer just aliases inputs and used to create big output tensor
        return input;
    }
}
