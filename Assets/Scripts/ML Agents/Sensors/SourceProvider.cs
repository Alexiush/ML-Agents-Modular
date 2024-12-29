using Unity.MLAgents.Sensors;
using UnityEngine;

namespace ModularMLAgents.Sensors
{
    public abstract class SourceProvider : MonoBehaviour
    {
        public abstract ISensor[] CreateSensors();

        public abstract int SourcesCount { get; }

        public abstract ObservationSpec ObservationSpec { get; }
    }
}
