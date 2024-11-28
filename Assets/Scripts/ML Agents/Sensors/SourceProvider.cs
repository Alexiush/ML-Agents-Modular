using Unity.MLAgents.Sensors;
using UnityEngine;

namespace ModularMLAgents.Sensors
{
    public abstract class SourceProvider : MonoBehaviour
    {
        public abstract ISensor CreateSensor();
    }
}
