using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using UnityEngine;
using ModularMLAgents.Sensors;
using ModularMLAgents.Components;

public class RollerSourcePositional : SourceProvider
{
    [SerializeField]
    private Agent _agent;
    [SerializeField]
    private Transform _target;

    public override ISensor[] CreateSensors()
    {
        return new ISensor[] { new RollerPositionalSensor(_agent, _target, 6, "SourcePositional") };
    }

    public override int SourcesCount => 1; 

    public override ObservationSpec ObservationSpec => ObservationSpec.Vector(6);
}

public class RollerPositionalSensor : ISensor
{
    private Agent _agent;
    private Transform _target;
    private VectorSensor _sensor;

    private ObservationSpec _observationSpec;
    private string _name;

    public RollerPositionalSensor(Agent agent, Transform target, int observationSize, string name = null,
        ObservationType observationType = ObservationType.Default)
    {
        _agent = agent;
        _target = target;
        _sensor = new VectorSensor(observationSize, name + "_vector", observationType);

        _name = name;
        _observationSpec = ObservationSpec.Vector(observationSize, observationType);
    }

    public int Write(ObservationWriter writer)
    {
        // _sensor.Reset();
        _sensor.AddObservation(_target.localPosition);
        _sensor.AddObservation(_agent.transform.localPosition);

        return _sensor.Write(writer);
    }

    public ObservationSpec GetObservationSpec()
    {
        return _observationSpec;
    }

    public void Update()
    {
        // In this framework I suggest not clearing the observations
        _sensor.Update();
    }

    public void Reset()
    {
        _sensor.Reset();
    }

    public string GetName()
    {
        return _name;
    }

    public virtual byte[] GetCompressedObservation()
    {
        return null;
    }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }
}
