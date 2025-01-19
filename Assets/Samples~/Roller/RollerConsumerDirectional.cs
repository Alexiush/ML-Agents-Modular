using ModularMLAgents.Components;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class RollerConsumerDirectional : ConsumerProvider
{
    [SerializeField]
    private Agent _agent;
    [SerializeField]
    private Transform _target;

    public override IActuator[] CreateActuators()
    {
        // Needs to create a vector sensor of size 8

        return new IActuator[] { new RollerActuatorDirectional(_agent, _target, "RollerConsumerDirectional") };
    }

    public override int ConsumersCount => 1;

    public override ActionSpec ActionSpec => ActionSpec.MakeContinuous(1);
}

public class RollerActuatorDirectional : IActuator
{
    private Agent _agent;
    private Transform _target;

    private string _name;

    public RollerActuatorDirectional(Agent agent, Transform target, string name)
    {
        _agent = agent;
        _target = target;

        _name = name;
    }

    public ActionSpec ActionSpec => ActionSpec.MakeContinuous(1);

    public string Name => _name;

    public void ResetData()
    {
        // No inner state
    }

    public float forceMultiplier = 10;

    public void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.y = actionBuffers.ContinuousActions[0];

        _agent.transform.rotation = Quaternion.Euler(controlSignal * 180);
    }

    public void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // No masking
    }
}
