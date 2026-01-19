using MLAgents.Configuration;
using UnityEngine;
using VYaml.Annotations;

namespace ModularMLAgents.Trainers
{
    [YamlObject]
    [System.Serializable]
    public abstract partial class CustomHyperparameters : Hyperparameters
    {
        [HideInInspector]
        public string PathToModel = string.Empty;
        [HideInInspector]
        public string PathToMapping = string.Empty;
    }
}
