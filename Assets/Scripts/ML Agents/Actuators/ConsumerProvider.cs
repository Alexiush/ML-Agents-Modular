using Unity.MLAgents.Actuators;
using UnityEngine;

namespace ModularMLAgents.Actuators
{
    public abstract class ConsumerProvider : MonoBehaviour
    {
        public abstract IActuator CreateActuator();

        public abstract ActionSpec ActionSpec { get; }
    }
}
