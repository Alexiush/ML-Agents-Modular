using ModularMLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples.Crawler;
using UnityEngine;

public class CrawlerLimbActuator : IActuator
{
    private CrawlerAgent _agent;
    private JointDriveController _jdController;
    private bool _isUpper;
    private Transform _bpTransform;

    private string _name;

    public CrawlerLimbActuator(CrawlerAgent agent, Transform bpTransform, bool isUpper, string name)
    {
        _agent = agent;
        _jdController = agent.JdController;
        _isUpper = isUpper;
        _bpTransform = bpTransform;

        _name = name;
    }

    public ActionSpec ActionSpec => _isUpper ? ActionSpec.MakeContinuous(3) : ActionSpec.MakeContinuous(2);

    public string Name => _name;

    public void ResetData()
    {
        // No inner state
    }

    public void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        int i = -1;

        var x = continuousActions[++i];
        var y = _isUpper ? continuousActions[++i] : 0;
        var s = continuousActions[++i];

        _jdController.bodyPartsDict[_bpTransform].SetJointTargetRotation(
            x, 
            y, 
            0
        );
        _jdController.bodyPartsDict[_bpTransform].SetJointStrength(s);
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
