using ModularMLAgents.Compilation;
using ModularMLAgents.Layers;
using ModularMLAgents.Models;
using ModularMLAgents.Sensors;
using System.Collections.Generic;
using Unity.Sentis;

[System.Serializable]
public class VectorInput : LayerBase, IEncoder
{
    public bool Normalize;

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
        compilationContext.AddDependencies("mlagents.trainers.torch_entities.encoders", "VectorInput");
        var layer = compilationContext.RegisterParameter("vector_input",
            $"VectorInput({inputShapes[0].Get(0)}, {(Normalize ? "True" : "False")})"
        );

        return $"self.{layer}(torch.flatten({input}, start_dim=0, end_dim=1)).view(-1, {outputDims[0].Compile()}, {inputShapes[0].Get(0)})";
    }

    public override List<DynamicTensorShape> GetShape(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return inputShapes;
    }

    public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return inputShapes.Count == 1 && inputShapes[0].rank == 1;
    }
}
