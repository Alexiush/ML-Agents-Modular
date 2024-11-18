using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VYaml.Annotations;

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
    public string InitPath = null;
    public bool Threaded = false;
}

[YamlObject]
[System.Serializable]
public partial class Configuration
{
    public List<Behavior> Behaviors = new List<Behavior>();
}
