using UnityEngine;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Serialization;

namespace ModularMLAgents.Trainers
{
    [YamlObjectUnion("!custom_ppo", typeof(CustomPPOTrainerHyperparameters))]
    public abstract partial class Hyperparameters { }

    [YamlObject]
    [System.Serializable]
    public partial class CustomPPOTrainerHyperparameters : Hyperparameters
    {
        public float Beta = 5e-3f;
        public float Epsilon = 0.2f;
        public LearningRateSchedule BetaSchedule = LearningRateSchedule.Linear;
        public LearningRateSchedule EpsilonSchedule = LearningRateSchedule.Linear;
        public float Lambd = 0.95f;
        public int NumEpoch = 3;
        public bool SharedCritic = false;

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            if (this is null)
            {
                emitter.WriteNull();
                return;
            }

            var formatter = StandardResolver.Instance.GetFormatterWithVerify<CustomPPOTrainerHyperparameters>();
            formatter.Serialize(ref emitter, this, context);
        }
    }

    [System.Serializable]
    public class CustomPPOTrainer : ITrainer
    {
        public CustomPPOTrainer() { }

        public string TrainerType => "custom_ppo";

        public CustomPPOTrainerHyperparameters CustomPPOHyperparameters;
        public Hyperparameters Hyperparameters => CustomPPOHyperparameters;

        public RewardSignals RewardSignals { get; set; }
    }
}
