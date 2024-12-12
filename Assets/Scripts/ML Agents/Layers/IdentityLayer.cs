using ModularMLAgents.Actuators;
using ModularMLAgents.Brain;
using ModularMLAgents.Compilation;
using ModularMLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;

namespace ModularMLAgents.Layers
{
    [System.Serializable]
    public class Identity : LayerBase, IDecoder, IEncoder
    {
        public override string Compile(CompilationContext compilationContext, List<TensorShape> inputShapes, List<TensorShape> outputShapes, string input)
        {
            return input;
        }

        public override List<TensorShape> GetShape(List<TensorShape> inputShapes, List<TensorShape> outputShapes)
        {
            return inputShapes;
        }

        public override bool Validate(List<TensorShape> inputShapes, List<TensorShape> outputShapes)
        {
            return true;
        }
    }

    [System.Serializable]
    public class IdentitySwitch : LayerBase, ISwitch
    {
        public override string Compile(CompilationContext compilationContext, List<TensorShape> inputShapes, List<TensorShape> outputShapes, string input)
        {
            return $"[{input}] * {outputShapes.Count}";
        }

        public override List<TensorShape> GetShape(List<TensorShape> inputShapes, List<TensorShape> outputShapes)
        {
            return outputShapes;
        }

        public override bool Validate(List<TensorShape> inputShapes, List<TensorShape> outputShapes)
        {
            return inputShapes.All(s => s.rank == 1) && outputShapes.All(s => s.rank == 1);
        }
    }
}
