using System.Linq;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace ModularMLAgents.Sensors
{
    public class ModularSensorManager : SensorComponent
    {
        [SerializeField]
        private ModularAgent _agent;

        private ISensor[] _sensors;

        private ISensor[] Sensors
        {
            get
            {
                if (_sensors == null)
                {
                    _sensors = _agent.SourceProviders
                        .OrderBy(s => s.Name)
                        .SelectMany(p => p.SourceProvider.CreateSensors()).ToArray();
                }

                return _sensors;
            }
        }

        public override ISensor[] CreateSensors()
        {
            return Sensors;
        }
    }
}
