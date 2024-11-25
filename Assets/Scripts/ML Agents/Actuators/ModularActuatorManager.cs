using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents;
using System.Linq;

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

    // I'm not sure of how they are usually combined
    public override ActionSpec ActionSpec => ActionSpec.Combine(Actuators.Select(a => a.ActionSpec).ToArray());
}
