using ModularMLAgents.Compilation;
using Unity.Sentis;

namespace ModularMLAgents.Actuators
{
    [System.Serializable]
    public abstract class Decoder
    {
        // Brain does not need to deal with the final output of effector, only match the signal that goes back
        // Thus decoder matches reduced input with the output

        public abstract string Compile(CompilationContext compilationContext, string input);

        public abstract TensorShape GetShape(TensorShape shape);
    }

    [System.Serializable]
    public class IdentityDecoder : Decoder
    {
        public override string Compile(CompilationContext compilationContext, string input)
        {
            return input;
        }

        public override TensorShape GetShape(TensorShape shape)
        {
            return shape;
        }
    }
}
