using ModularMLAgents.Components;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class RollerConsumer : ConsumerProvider
{
    [SerializeField]
    private Agent _agent;
    [SerializeField]
    private Transform _target;

    public override IActuator[] CreateActuators()
    {
        return new IActuator[] { new RollerActuator(_agent, _target, name: name) };
    }

    public override int ConsumersCount => 1;

    public override ActionSpec ActionSpec => ActionSpec.MakeContinuous(2);
}
