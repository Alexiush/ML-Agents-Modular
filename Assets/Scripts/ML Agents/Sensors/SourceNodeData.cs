using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

[System.Serializable]
public class SourceNodeData : AgentGraphNodeData
{
    [SerializeField] 
    public InputSource Source;

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
        var input = $"input_tensor[{compilationContext.GetSourceNumber(this)}]";

        // Source should be taking its tensor by index
        return input;
    }

    public override InplaceArray<int> GetShape(CompilationContext compilationContext)
    {
        return Source.Schema.ToShape();
    }
}
