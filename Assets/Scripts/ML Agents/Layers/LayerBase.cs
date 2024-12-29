using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using System.Collections.Generic;
using Unity.Sentis;

namespace ModularMLAgents.Layers
{
    public interface ILayer
    {
        public LayerBase Layer { get; }
    }

    public abstract class LayerBase : ILayer
    {
        public LayerBase Layer => this;

        public abstract List<SymbolicTensorDim> SymbolicForwardPass(List<SymbolicTensorDim> dims);

        public abstract List<SymbolicTensorDim> SymbolicBackwardPass(List<SymbolicTensorDim> dims);

        public abstract string Compile(CompilationContext compilationContext,
            List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes,
            List<SymbolicTensorDim> inputDims, List<SymbolicTensorDim> outputDims,
            string input);

        public abstract List<DynamicTensorShape> GetShape(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes);

        public abstract bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes);
    }
}
