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

    public override IActuator[] CreateActuators()
    {
        return _agent.ConsumerProviders.Select(p => p.ConsumerProvider.CreateActuator()).ToArray();
    }

    // I'm not sure of how they are usually combined
    public override ActionSpec ActionSpec => new ActionSpec();
}
