using ModularMLAgents.Compilation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;
using static Unity.Sentis.Model;

namespace ModularMLAgents.Brain
{
    [System.Serializable]
    public class Brain
    {
        // Brain node is the central part of the agent
        // It processes signals it gets from the sensors and based on that it activates the effectors

        // Here approach is unique as the brain on itself decides what kind of input is important for what effector
        // as he process all the different data it has  

        [SubclassSelector, SerializeReference]
        public Switch Switch = new MLPSwitch();
    }

    public abstract class Switch
    {
        public abstract string Compile(CompilationContext compilationContext, TensorShape inputShape, List<TensorShape> outputShapes, string input);
    }

    [System.Serializable]
    public class MLPSwitch : Switch
    {
        public int HiddenLayers = 2;
        public int HiddenSize = 128;
        public int SelectorLayers = 1;

        public override string Compile(CompilationContext compilationContext, TensorShape inputShape, List<TensorShape> outputShapes, string input)
        {
            compilationContext.AddDependency("custom_ppo_plugin.layers.brain_mlp", "BrainMLP");

            var layer = compilationContext.RegisterParameter("brain",
                $"BrainMLP({inputShape}, {HiddenLayers}, {HiddenSize}, {SelectorLayers}, [{string.Join(", ", outputShapes.Select(s => s[0]))}])"
            );

            return $"self.{layer}({input})";
        }
    }

    [System.Serializable]
    public class MLPSwitchHardSelector : MLPSwitch
    {
        public override string Compile(CompilationContext compilationContext, TensorShape inputShape, List<TensorShape> outputShapes, string input)
        {
            compilationContext.AddDependency("custom_ppo_plugin.layers.brain_mlp", "BrainHardSelection");

            var layer = compilationContext.RegisterParameter("brain",
                $"BrainHardSelection({inputShape}, {HiddenLayers}, {HiddenSize}, {SelectorLayers}, [{string.Join(", ", outputShapes.Select(s => s[0]))}])"
            );

            return $"self.{layer}({input})";
        }
    }

    [System.Serializable]
    public class IdentitySwitch : Switch
    {
        public override string Compile(CompilationContext compilationContext, TensorShape inputShape, List<TensorShape> outputShapes, string input)
        {
            return $"[{input}] * {outputShapes.Count}";
        }
    }
}
