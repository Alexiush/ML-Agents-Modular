using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

[System.Serializable]
public abstract class Encoder
{
    // Base class for encoding architecture, passing raw data to the brain is not the best idea
    // It also won't allow to merge different data, so it needs to be encoded into the latent space

    public abstract string Compile(CompilationContext compilationContext, string input);
}

[System.Serializable]
public class IdentityEncoder : Encoder
{
    public override string Compile(CompilationContext compilationContext, string input)
    {
        return input;
    }
}

[System.Serializable]
public class LinearEncoder : Encoder
{
    // Input size is to be controlled by schema
    private int InputSize => 8;

    public int NumLayers = 2;
    public int HiddenSize = 128;
    public Initialization KernelInitialization = Initialization.KaimingHeNormal;
    public float KernelGain = 1.0f;

    public override string Compile(CompilationContext compilationContext, string input)
    {
        compilationContext.AddDependency("mlagents.trainers.torch_entities.layers", "LinearEncoder");
        return $"LinearEncoder({InputSize}, {NumLayers}, {HiddenSize}, Initialization.{KernelInitialization}, {KernelGain})({input})";
    }
}
