using ModularMLAgents.Actuators;
using ModularMLAgents.Brain;
using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using ModularMLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;

namespace ModularMLAgents.Layers
{
    [System.Serializable]
    public class Identity : LayerBase, IDecoder, IEncoder
    {
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
            return input;
        }

        public override List<DynamicTensorShape> GetShape(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
        {
            return inputShapes;
        }

        public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
        {
            return true;
        }
    }

    [System.Serializable]
    public class IdentitySwitch : LayerBase, ISwitch
    {
        public override List<SymbolicTensorDim> SymbolicForwardPass(List<SymbolicTensorDim> dims)
        {
            return new List<SymbolicTensorDim>();
        }

        public override List<SymbolicTensorDim> SymbolicBackwardPass(List<SymbolicTensorDim> dims)
        {
            return new List<SymbolicTensorDim>();
        }

        public override string Compile(CompilationContext compilationContext,
            List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes,
            List<SymbolicTensorDim> inputDims, List<SymbolicTensorDim> outputDims,
            string input)
        {
            var reshapedInputs = inputDims
                .Zip(outputShapes, (d, s) => (d, s))
                .Select(ds => $"({input}).expand(-1, {ds.d.Compile()}, -1)");

            return $"[{string.Join(", ", reshapedInputs)}]";
        }

        public override List<DynamicTensorShape> GetShape(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
        {
            return outputShapes;
        }

        public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
        {
            return inputShapes.All(s => s.rank == 1) && outputShapes.All(s => s.rank == 1);
        }
    }
}
