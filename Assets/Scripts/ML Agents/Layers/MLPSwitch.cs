using ModularMLAgents.Brain;
using ModularMLAgents.Compilation;
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

        public override string Compile(CompilationContext compilationContext, List<TensorShape> inputShapes, List<TensorShape> outputShapes, string input)
        {
            compilationContext.AddDependencies("custom_ppo_plugin.layers.brain_mlp", "BrainMLP");

            var inputShapeMerged = inputShapes.Select(s => s[0]).Sum();
            var layer = compilationContext.RegisterParameter("brain",
                $"BrainMLP({inputShapeMerged}, {HiddenLayers}, {HiddenSize}, {SelectorLayers}, [{string.Join(", ", outputShapes.Select(s => s[0]))}])"
            );

            return $"self.{layer}({input})";
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

    [System.Serializable]
    public class MLPSwitchHardSelector : MLPSwitch
    {
        public override string Compile(CompilationContext compilationContext, List<TensorShape> inputShapes, List<TensorShape> outputShapes, string input)
        {
            compilationContext.AddDependencies("custom_ppo_plugin.layers.brain_mlp", "BrainHardSelection");

            var inputShapeMerged = inputShapes.Select(s => s[0]).Sum();
            var layer = compilationContext.RegisterParameter("brain",
                $"BrainHardSelection({inputShapeMerged}, {HiddenLayers}, {HiddenSize}, {SelectorLayers}, [{string.Join(", ", outputShapes.Select(s => s[0]))}])"
            );

            return $"self.{layer}({input})";
        }
    }
}
