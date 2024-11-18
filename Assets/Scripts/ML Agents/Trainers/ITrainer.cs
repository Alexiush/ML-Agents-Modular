using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Serialization;

public enum LearningRateSchedule
{
    Linear,
    Constant
}

[YamlObject]
[System.Serializable]
public abstract partial class Hyperparameters
{
    public float LearningRate = 3e-4f;
    public int BatchSize = 32;
    public int BufferSize = 320;
    public LearningRateSchedule LearningRateSchedule = LearningRateSchedule.Linear;

    public abstract void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context);
}

public interface ITrainer
{
    public string TrainerType { get; }
    public Hyperparameters Hyperparameters { get; }
}
