using System.Collections.Generic;
using VYaml.Annotations;
using ModularMLAgents.Trainers;
using UnityEngine;
using System;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;
using ModularMLAgents.Configuration;
using ModularMLAgents.Utilities;

namespace ModularMLAgents.Configuration
{
    [YamlObject]
    [System.Serializable]
    public partial class Behavior
    {
        [YamlIgnore]
        public string BehaviorId = string.Empty;

        public string TrainerType = string.Empty;
        public Hyperparameters Hyperparameters;
        public NetworkSettings NetworkSettings = new NetworkSettings();
        public RewardSignals RewardSignals = new RewardSignals();

        public int SummaryFreq = 50000;
        public int TimeHorizon = 64;
        public int MaxSteps = 500000;
        public int KeepCheckpoints = 5;
        public bool EvenCheckpoints = false;
        public int CheckpointInterval = 500000;
        public NullWhenEmptyString InitPath = null;
        public bool Threaded = false;
    }

    public class BehaviorFormatter : WriteOnlyYamlFormatter<Behavior>
    {
        public override void Serialize(ref Utf8YamlEmitter emitter, Behavior value, YamlSerializationContext context)
        {
            emitter.BeginMapping();
            {
                // Proceed as usual with all YamlProperties except for reward signals 
                // Which are only mapped if any of the signals is used

                emitter.WriteString("trainer_type");
                context.Serialize(ref emitter, value.TrainerType);

                emitter.WriteString("hyperparameters");
                context.Serialize(ref emitter, value.Hyperparameters);

                emitter.WriteString("network_settings");
                context.Serialize(ref emitter, value.NetworkSettings);

                // Reward signals
                if (value.RewardSignals.UseExtrinsic
                    || value.RewardSignals.UseCuriosity
                    || value.RewardSignals.UseGAIL
                    || value.RewardSignals.UseRND)
                {
                    emitter.WriteString("reward_signals");
                    context.Serialize(ref emitter, value.RewardSignals);
                }

                emitter.WriteString("summary_freq");
                context.Serialize(ref emitter, value.SummaryFreq);

                emitter.WriteString("time_horizon");
                context.Serialize(ref emitter, value.TimeHorizon);

                emitter.WriteString("max_steps");
                context.Serialize(ref emitter, value.MaxSteps);

                emitter.WriteString("keep_checkpoints");
                context.Serialize(ref emitter, value.KeepCheckpoints);

                emitter.WriteString("even_checkpoints");
                context.Serialize(ref emitter, value.EvenCheckpoints);

                emitter.WriteString("checkpoint_interval");
                context.Serialize(ref emitter, value.CheckpointInterval);

                emitter.WriteString("init_path");
                context.Serialize(ref emitter, value.InitPath);

                emitter.WriteString("threaded");
                context.Serialize(ref emitter, value.Threaded);
            }
            emitter.EndMapping();
        }
    }
}
