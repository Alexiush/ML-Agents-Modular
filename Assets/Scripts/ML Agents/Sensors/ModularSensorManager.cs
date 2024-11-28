using System.Linq;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace ModularMLAgents.Sensors
{
    public class ModularSensorManager : SensorComponent
    {
        [SerializeField]
        private ModularAgent _agent;

        public override ISensor[] CreateSensors()
        {
            return _agent.SourceProviders.Select(p => p.SourceProvider.CreateSensor()).ToArray();
        }
    }
}
