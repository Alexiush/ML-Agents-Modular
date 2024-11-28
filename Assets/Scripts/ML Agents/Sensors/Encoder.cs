using ModularMLAgents.Compilation;
using ModularMLAgents.Utilities;
using Unity.Sentis;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public abstract class Encoder
    {
        // Base class for encoding architecture, passing raw data to the brain is not the best idea
        // It also won't allow to merge different data, so it needs to be encoded into the latent space

        public abstract string Compile(CompilationContext compilationContext, TensorShape shape, string input);
        public abstract TensorShape GetShape(TensorShape shape);
    }

    [System.Serializable]
    public class IdentityEncoder : Encoder
    {
        public override string Compile(CompilationContext compilationContext, TensorShape shape, string input)
        {
            return input;
        }

        public override TensorShape GetShape(TensorShape shape)
        {
            return shape;
        }
    }

    [System.Serializable]
    public class LinearEncoder : Encoder
    {
        // Input size is to be inferred automatically

        public int NumLayers = 2;
        public int HiddenSize = 128;
        public Initialization KernelInitialization = Initialization.KaimingHeNormal;
        public float KernelGain = 1.0f;

        public override string Compile(CompilationContext compilationContext, TensorShape shape, string input)
        {
            compilationContext.AddDependency("mlagents.trainers.torch_entities.layers", "LinearEncoder");
            var layer = compilationContext.RegisterParameter("linear",
                $"LinearEncoder({shape[0]}, {NumLayers}, {HiddenSize}, Initialization.{KernelInitialization}, {KernelGain})"
            );

            return $"self.{layer}({input})";
        }

        public override TensorShape GetShape(TensorShape shape)
        {
            return new TensorShape(HiddenSize);
        }
    }
}
