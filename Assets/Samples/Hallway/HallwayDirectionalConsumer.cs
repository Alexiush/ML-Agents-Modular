using ModularMLAgents.Actuators;
using ModularMLAgents.Components;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class HallwayDirectionalConsumer : ConsumerProvider
{
    [SerializeField]
    private HallwayAgent _agent;

    public override IActuator[] CreateActuators()
    {
        return new IActuator[] { new HallwayActuatorDirectional(_agent, "HallwayActuatorDirectional") };
    }

    public override int ConsumersCount => 1;

    public override ActionSpec ActionSpec => ActionSpec.MakeDiscrete(new int[] { 5 });
}

public class HallwayActuatorDirectional : IActuator
{
    private HallwayAgent _agent;
    private string _name;

    public HallwayActuatorDirectional(HallwayAgent agent, string name)
    {
        _agent = agent;
        _name = name;
    }

    public ActionSpec ActionSpec => ActionSpec.MakeDiscrete(new int[] { 5 });

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

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = _agent.transform.forward * 1f;
                break;
            case 2:
                dirToGo = _agent.transform.forward * -1f;
                break;
            case 3:
                rotateDir = _agent.transform.up * 1f;
                break;
            case 4:
                rotateDir = _agent.transform.up * -1f;
                break;
        }
        _agent.transform.Rotate(rotateDir, Time.deltaTime * 150f);
        _agent.AgentRb.AddForce(dirToGo * _agent.HallwaySettings.agentRunSpeed, ForceMode.VelocityChange);

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
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // No masking
    }
}
