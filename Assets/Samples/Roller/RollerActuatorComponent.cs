using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RollerActuatorComponent : ActuatorComponent
{
    [SerializeField]
    private Agent _agent;
    [SerializeField]
    private Transform _target;

    public override IActuator[] CreateActuators()
    {
        // Needs to create a vector sensor of size 8

        return new[]
        {
            new RollerActuator(_agent, _target, name: name)
        };
    }

    public override ActionSpec ActionSpec => ActionSpec.MakeContinuous(2);
}

public class RollerActuator : IActuator
{
    private Agent _agent;
    private Transform _target;
    private Rigidbody _rigidbody;

    private string _name;

    public RollerActuator(Agent agent, Transform target, string name)
    {
        _agent = agent;
        _target = target;
        _rigidbody = agent.GetComponent<Rigidbody>();

        _name = name;
    }

    public ActionSpec ActionSpec => ActionSpec.MakeContinuous(2);

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
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        _rigidbody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(_agent.transform.localPosition, _target.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            _agent.SetReward(1.0f);
            _agent.EndEpisode();
        }

        // Fell off platform
        else if (_agent.transform.localPosition.y < 0)
        {
            _agent.EndEpisode();
        }
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
