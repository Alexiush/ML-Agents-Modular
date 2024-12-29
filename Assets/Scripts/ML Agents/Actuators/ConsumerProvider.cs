using Unity.MLAgents.Actuators;
using UnityEngine;

namespace ModularMLAgents.Actuators
{
    public abstract class ConsumerProvider : MonoBehaviour
    {
        public abstract IActuator[] CreateActuators();

        public abstract int ConsumersCount { get; }

        public abstract ActionSpec ActionSpec { get; }
    }
}
