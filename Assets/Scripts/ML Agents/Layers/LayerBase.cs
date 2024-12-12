using ModularMLAgents.Compilation;
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

        public abstract string Compile(CompilationContext compilationContext, List<TensorShape> inputShapes, List<TensorShape> outputShapes, string input);

        public abstract List<TensorShape> GetShape(List<TensorShape> inputShapes, List<TensorShape> outputShapes);

        public abstract bool Validate(List<TensorShape> inputShapes, List<TensorShape> outputShapes);
    }
}
