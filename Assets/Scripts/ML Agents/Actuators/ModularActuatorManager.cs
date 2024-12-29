using System.Linq;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace ModularMLAgents.Actuators
{
    public class ModularActuatorManager : ActuatorComponent
    {
        [SerializeField]
        private ModularAgent _agent;

        private IActuator[] _actuators;

        private IActuator[] Actuators
        {
            get
            {
                if (_actuators == null)
                {
                    _actuators = _agent.ConsumerProviders.SelectMany(p => p.ConsumerProvider.CreateActuators()).ToArray();
                }

                return _actuators;
            }
        }

        public override IActuator[] CreateActuators()
        {
            return Actuators;
        }

        public override ActionSpec ActionSpec => ActionSpec.Combine(Actuators.Select(a => a.ActionSpec).ToArray());
    }
}
