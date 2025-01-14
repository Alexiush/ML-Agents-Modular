using ModularMLAgents.Compilation;
using ModularMLAgents.Layers;
using ModularMLAgents.Models;
using ModularMLAgents.Sensors;
using System.Collections.Generic;
using Unity.Sentis;

[System.Serializable]
public class EntityEmbedding : LayerBase, IEncoder
{
    public int AttentionSize = 128;
    public int MaxEntities = 1;

    public override List<SymbolicTensorDim> SymbolicForwardPass(List<SymbolicTensorDim> dims)
    {
        return dims;
    }

    public override List<SymbolicTensorDim> SymbolicBackwardPass(List<SymbolicTensorDim> dims)
    {
        return dims;
    }

    public override string Compile(CompilationContext compilationContext,
        List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes,
        List<SymbolicTensorDim> inputDims, List<SymbolicTensorDim> outputDims,
        string input)
    {
        compilationContext.AddDependencies("mlagents.trainers.torch_entities.attention", "EntityEmbedding", "ResidualSelfAttention", "get_zero_entities_mask");
        var embedding = compilationContext.RegisterParameter("entity_embedding",
            $"EntityEmbedding({inputShapes[0].Get(1)}, {inputShapes[0].Get(0)}, {AttentionSize})"
        );

        var rsa = compilationContext.RegisterParameter("rsa",
            $"ResidualSelfAttention({AttentionSize}, {MaxEntities})"
        );

        return $"self.{rsa}(self.{embedding}(None, torch.flatten({input}, start_dim=0, end_dim=1)), get_zero_entities_mask({input})).view(-1, {outputDims[0].Compile()}, {AttentionSize})";
    }

    public override List<DynamicTensorShape> GetShape(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return new List<DynamicTensorShape> { new DynamicTensorShape(AttentionSize) };
    }

    public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return inputShapes.Count == 1 && inputShapes[0].rank == 2;
    }
}
