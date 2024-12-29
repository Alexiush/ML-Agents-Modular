using ModularMLAgents.Actuators;
using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using ModularMLAgents.Sensors;
using ModularMLAgents.Utilities;
using System.Collections.Generic;
using Unity.Sentis;

namespace ModularMLAgents.Layers
{
    [System.Serializable]
    public class LinearLayer : LayerBase, IEncoder, IDecoder
    {
        // Input size is to be inferred automatically

        public int NumLayers = 2;
        public int HiddenSize = 128;
        public Initialization KernelInitialization = Initialization.KaimingHeNormal;
        public float KernelGain = 1.0f;

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
            compilationContext.AddDependencies("mlagents.trainers.torch_entities.layers", "LinearEncoder");
            var layer = compilationContext.RegisterParameter("linear",
                $"LinearEncoder({inputShapes[0].Get(0)}, {NumLayers}, {HiddenSize}, Initialization.{KernelInitialization}, {KernelGain})"
            );

            return $"self.{layer}(torch.flatten({input}, start_dim=0, end_dim=1)).view(-1, {outputDims[0].Compile()}, {HiddenSize})";
        }

        public override List<DynamicTensorShape> GetShape(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
        {
            return new List<DynamicTensorShape> { new DynamicTensorShape(HiddenSize) };
        }

        public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
        {
            return inputShapes.Count == 1 && inputShapes[0].rank == 1;
        }
    }
}
