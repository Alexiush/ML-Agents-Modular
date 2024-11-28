using UnityEngine;
using Unity.MLAgents.Actuators;
using System.Linq;

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
                    _actuators = _agent.ConsumerProviders.Select(p => p.ConsumerProvider.CreateActuator()).ToArray();
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
