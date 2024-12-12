using ModularMLAgents.Actuators;
using ModularMLAgents.Compilation;
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

        public override string Compile(CompilationContext compilationContext, List<TensorShape> inputShapes, List<TensorShape> outputShapes, string input)
        {
            compilationContext.AddDependencies("mlagents.trainers.torch_entities.layers", "LinearEncoder");
            var layer = compilationContext.RegisterParameter("linear",
                $"LinearEncoder({inputShapes[0][0]}, {NumLayers}, {HiddenSize}, Initialization.{KernelInitialization}, {KernelGain})"
            );

            return $"self.{layer}({input})";
        }

        public override List<TensorShape> GetShape(List<TensorShape> inputShapes, List<TensorShape> outputShapes)
        {
            return new List<TensorShape> { new TensorShape(HiddenSize) };
        }

        public override bool Validate(List<TensorShape> inputShapes, List<TensorShape> outputShapes)
        {
            return inputShapes.Count == 1 && inputShapes[0].rank == 1;
        }
    }
}
