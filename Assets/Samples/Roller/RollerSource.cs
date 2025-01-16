using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using ModularMLAgents.Sensors;
using ModularMLAgents.Components;

public class RollerSource : SourceProvider
{
    [SerializeField]
    private Agent _agent;
    [SerializeField]
    private Transform _target;

    public override ISensor[] CreateSensors()
    {
        return new ISensor[] { new RollerSensor(_agent, _target, 8, name: name) };
    }

    public override int SourcesCount => 1;

    public override ObservationSpec ObservationSpec => ObservationSpec.Vector(8);
}
