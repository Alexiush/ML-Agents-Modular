using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

[System.Serializable]
public class ConsumerNodeData : AgentGraphNodeData
{
    [SerializeField] 
    public Consumer Consumer;

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
        compilationContext.RegisterActionModel($"ActionModel({Consumer.Schema.Dimensions[0]}, ActionSpec({Consumer.ActionSpec.NumContinuousActions}, ({string.Join(", ", Consumer.ActionSpec.BranchSizes)})))");

        // Consumer just aliases inputs and used to create big output tensor
        return input;
    }

    public override InplaceArray<int> GetShape(CompilationContext compilationContext)
    {
        // Not yet getting spec from consumers
        throw new System.NotImplementedException();
    }
}
