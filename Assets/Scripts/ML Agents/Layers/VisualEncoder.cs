using ModularMLAgents.Compilation;
using ModularMLAgents.Layers;
using ModularMLAgents.Models;
using ModularMLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;

[System.Serializable]
public abstract class VisualEncoderBase : LayerBase, IEncoder
{
    public int HiddenSize = 128;

    public override List<SymbolicTensorDim> SymbolicForwardPass(List<SymbolicTensorDim> dims)
    {
        return dims;
    }

    public override List<SymbolicTensorDim> SymbolicBackwardPass(List<SymbolicTensorDim> dims)
    {
        return dims;
    }

    protected abstract string EncoderClass { get; }

    public override string Compile(CompilationContext compilationContext,
        List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes,
        List<SymbolicTensorDim> inputDims, List<SymbolicTensorDim> outputDims,
        string input)
    {
        compilationContext.AddDependencies("mlagents.trainers.torch_entities.encoders", EncoderClass);
        var layer = compilationContext.RegisterParameter("vector_input",
            $"{EncoderClass}({inputShapes[0].Get(1)}, {inputShapes[0].Get(2)}, {inputShapes[0].Get(0)}, {HiddenSize})"
        );

        return $"self.{layer}(torch.flatten({input}, start_dim=0, end_dim=1)).view(-1, {outputDims[0].Compile()}, {HiddenSize})";
    }

    public override List<DynamicTensorShape> GetShape(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return new List<DynamicTensorShape> { new DynamicTensorShape(HiddenSize) };
    }

    public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return inputShapes.Count == 1 && inputShapes[0].rank == 3;
    }
}

[System.Serializable]
public class SimpleVisualEncoder : VisualEncoderBase
{
    protected override string EncoderClass => "SimpleVisualEncoder";

    public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return base.Validate(inputShapes, outputShapes) && inputShapes[0].Get(1) >= 20 && inputShapes[0].Get(2) >= 20;
    }
}

[System.Serializable]
public class ResNetVisualEncoder : VisualEncoderBase
{
    protected override string EncoderClass => "ResNetVisualEncoder";

    public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return base.Validate(inputShapes, outputShapes) && inputShapes[0].Get(1) >= 15 && inputShapes[0].Get(2) >= 15;
    }
}

[System.Serializable]
public class NatureVisualEncoder : VisualEncoderBase
{
    protected override string EncoderClass => "NatureVisualEncoder";

    public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        Debug.Log(inputShapes[0]);
        return base.Validate(inputShapes, outputShapes) && inputShapes[0].Get(1) >= 36 && inputShapes[0].Get(2) >= 36;
    }
}

[System.Serializable]
public class SmallVisualEncoder : VisualEncoderBase
{
    protected override string EncoderClass => "SmallVisualEncoder";

    public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
    {
        return base.Validate(inputShapes, outputShapes) && inputShapes[0].Get(1) >= 5 && inputShapes[0].Get(2) >= 5;
    }
}

[System.Serializable]
public class FullyConnectedVisualEncoder : VisualEncoderBase
{
    protected override string EncoderClass => "FullyConnectedVisualEncoder";
}