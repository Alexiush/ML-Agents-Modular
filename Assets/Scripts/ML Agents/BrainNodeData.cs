using System.Linq;
using Unity.MLAgents;
using Unity.Sentis;
using UnityEngine;

[System.Serializable]
public class BrainNodeData : AgentGraphNodeData
{
    [SerializeField] public Brain Brain;

    public override AgentGraphNode Load()
    {
        var brainNode = new BrainNode(Metadata);
        brainNode.SetPosition(Metadata.Position);
        Ports.ForEach(p => p.Instantiate(brainNode));

        return brainNode;
    }

    public override string GetExpressionBody(CompilationContext compilationContext)
    {
        // For now: get inputs
        var inputs = compilationContext
            .GetInputNodes(this);

        var inputReferences = inputs
            .Select(i => compilationContext.GetReference(i));

        var inputShapeMerged = inputs
            .Select(n => n.GetShape(compilationContext)[0])
            .Sum();

        // Work as linear
        var layer = compilationContext.RegisterParameter("linear",
            $"LinearEncoder({inputShapeMerged}, 2, 128, Initialization.KaimingHeNormal, 1)"
        );
        compilationContext.AddDependency("mlagents.trainers.torch_entities.layers", "LinearEncoder");
        
        var input = $"torch.cat([{string.Join(", ", inputReferences)}], dim = 1)";
        return $"self.{layer}({input})";
    }

    public override InplaceArray<int> GetShape(CompilationContext compilationContext)
    {
        /*
        var inputShapes = compilationContext
            .GetInputNodes(this)
            .Select(n => n.GetShape(compilationContext));
        */

        // For now brain is linear-only
        return new InplaceArray<int>(128);
    }
}
