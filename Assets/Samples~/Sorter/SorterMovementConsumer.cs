using ModularMLAgents.Components;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SorterMovementConsumer : ConsumerProvider
{
    [SerializeField]
    private SorterAgent _agent;
    [SerializeField]
    private Transform _target;

    public override IActuator[] CreateActuators()
    {
        return new IActuator[] { new SorterActuatorDirectional(_agent, "SorterActuatorDirectional") };
    }

    public override int ConsumersCount => 1;

    public override ActionSpec ActionSpec => ActionSpec.MakeDiscrete(new int[] { 3, 3, 3 });
}

public class SorterActuatorDirectional : IActuator
{
    private SorterAgent _agent;
    private string _name;

    public SorterActuatorDirectional(SorterAgent agent, string name)
    {
        _agent = agent;
        _name = name;
    }

    public ActionSpec ActionSpec => ActionSpec.MakeDiscrete(new int[] { 3, 3, 3 });

    public string Name => _name;

    public void ResetData()
    {
        // No inner state
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = _agent.transform.forward * 1f;
                break;
            case 2:
                dirToGo = _agent.transform.forward * -1f;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = _agent.transform.right * 1f;
                break;
            case 2:
                dirToGo = _agent.transform.right * -1f;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = _agent.transform.up * -1f;
                break;
            case 2:
                rotateDir = _agent.transform.up * 1f;
                break;
        }

        _agent.transform.Rotate(rotateDir, Time.deltaTime * 200f);
        _agent.AgentRb.AddForce(dirToGo * 2, ForceMode.VelocityChange);

    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public void OnActionReceived(ActionBuffers actionBuffers)

    {
        // Move the agent using the action.
        MoveAgent(actionBuffers.DiscreteActions);

        // Penalty given each step to encourage agent to finish task quickly.
        _agent.AddReward(-1f / _agent.MaxStep);
    }

    public void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }
    }

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // No masking
    }
}
