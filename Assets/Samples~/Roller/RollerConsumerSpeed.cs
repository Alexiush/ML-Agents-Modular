using ModularMLAgents.Components;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class RollerConsumerSpeed : ConsumerProvider
{
    [SerializeField]
    private Agent _agent;
    [SerializeField]
    private Transform _target;

    public override IActuator[] CreateActuators()
    {
        // Needs to create a vector sensor of size 8

        return new IActuator[] { new RollerActuatorSpeed(_agent, _target, "RollerConsumerSpeed") };
    }

    public override int ConsumersCount => 1;

    public override ActionSpec ActionSpec => ActionSpec.MakeContinuous(1);
}

public class RollerActuatorSpeed : IActuator
{
    private Agent _agent;
    private Transform _target;
    private Rigidbody _rigidbody;

    private string _name;

    public RollerActuatorSpeed(Agent agent, Transform target, string name)
    {
        _agent = agent;
        _target = target;
        _rigidbody = agent.GetComponent<Rigidbody>();

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
        var forward = _agent.transform.forward;
        forward.y = 0;
        var speed = actionBuffers.ContinuousActions[0];

        _rigidbody.AddForce(forward.normalized * speed * forceMultiplier);

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
        throw new System.NotImplementedException();
    }

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // No masking
    }
}
