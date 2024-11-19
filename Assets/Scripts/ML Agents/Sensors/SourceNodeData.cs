using System.Linq;
using UnityEngine;

[System.Serializable]
public class SourceNodeData : AgentGraphNodeData
{
    [SerializeField] public InputSource Source;

    public override AgentGraphNode Load()
    {
        var sourceNode = new SourceNode(Metadata);
        sourceNode.SetPosition(Metadata.Position);
        Ports.ForEach(p => p.Instantiate(sourceNode));

        return sourceNode;
    }

    public override string GetExpressionBody(CompilationContext compilationContext)
    {
        // Input variable is predefined
        var input = "input_tensor";

        // Source should slice and reshape, but identity for now
        return input;
    }
}
