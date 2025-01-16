using MLAgents.Configuration;
using UnityEngine;
using VYaml.Annotations;

/*
namespace MLAgents.Configuration
{
    [YamlObjectUnion("!custom", typeof(ModularMLAgents.Trainers.CustomHyperparameters))]
    public abstract partial class Hyperparameters { }
}
*/

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
