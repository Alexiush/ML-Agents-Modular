using ModularMLAgents.Brain;
using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;

namespace ModularMLAgents.Layers
{
    [System.Serializable]
    public class MLPSwitch : LayerBase, ISwitch
    {
        public int HiddenLayers = 2;
        public int HiddenSize = 128;
        public int SelectorLayers = 1;

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
            compilationContext.AddDependencies("custom_ppo_plugin.layers.brain_mlp", "BrainMLP");

            var inputShapeMerged = new SumSymbolicTensorDim(inputShapes
                .Zip(outputDims,
                    (s, d) => new ProductSymbolicTensorDim(new List<SymbolicTensorDim>() { new DefinedSymbolicTensorDim(s.Get(0)), d }))
                .Cast<SymbolicTensorDim>()
                .ToList()
            );

            var layer = compilationContext.RegisterParameter("brain",
                $"BrainMLP({inputShapeMerged.Compile()}, {HiddenLayers}, {HiddenSize}, {SelectorLayers}, [{string.Join(", ", outputShapes.Select(s => s.Get(0)))}], [{string.Join(", ", outputDims.Select(s => s.Compile()))}])"
            );

            return $"self.{layer}({input})";
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

    [System.Serializable]
    public class MLPSwitchHardSelector : MLPSwitch
    {
        public override string Compile(CompilationContext compilationContext,
            List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes,
            List<SymbolicTensorDim> inputDims, List<SymbolicTensorDim> outputDims,
            string input)
        {
            compilationContext.AddDependencies("custom_ppo_plugin.layers.brain_mlp", "BrainHardSelection");

            var inputShapeMerged = new SumSymbolicTensorDim(inputShapes
                .Zip(outputDims,
                    (s, d) => new ProductSymbolicTensorDim(new List<SymbolicTensorDim>() { new DefinedSymbolicTensorDim(s.Get(0)), d }))
                .Cast<SymbolicTensorDim>()
                .ToList()
            );

            var layer = compilationContext.RegisterParameter("brain",
                $"BrainHardSelection({inputShapeMerged.Compile()}, {HiddenLayers}, {HiddenSize}, {SelectorLayers}, [{string.Join(", ", outputShapes.Select(s => s.Get(0)))}], [{string.Join(", ", outputDims.Select(s => s.Compile()))}])"
            );

            return $"self.{layer}({input})";
        }
    }
}
