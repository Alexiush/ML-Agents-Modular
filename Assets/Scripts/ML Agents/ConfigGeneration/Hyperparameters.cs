using ModularMLAgents.Utilities;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Serialization;

namespace ModularMLAgents.Configuration
{
    public enum LearningRateSchedule
    {
        Linear,
        Constant
    }

    [YamlObject]
    [System.Serializable]
    public abstract partial class Hyperparameters : IDynamicallyYAMLSerialized
    {
        public float LearningRate = 3e-4f;
        public int BatchSize = 32;
        public int BufferSize = 320;
        public LearningRateSchedule LearningRateSchedule = LearningRateSchedule.Linear;

        public abstract void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context);
    }

    public class HyperparametersFormatter : WriteOnlyYamlFormatter<Hyperparameters>
    {
        public override void Serialize(ref Utf8YamlEmitter emitter, Hyperparameters value, YamlSerializationContext context)
        {
            value.Serialize(ref emitter, context);
        }
    }
}
