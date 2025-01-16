using ModularMLAgents.Layers;
using UnityEngine;

namespace ModularMLAgents.Brain
{
    [System.Serializable]
    public class Brain
    {
        // Brain node is the central part of the agent
        // It processes signals it gets from the sensors and based on that it activates the effectors

        // Here approach is unique as the brain on itself decides what kind of input is important for what effector
        // as he process all the different data it has  

        [SubclassSelector, SerializeReference]
        public ISwitch Switch = new MLPSwitch();
    }

    public interface ISwitch : ILayer { }
}
