using ModularMLAgents.Components;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SorterDirectionalSource : SourceProvider
{
    [SerializeField]
    private Agent _agent;
    [SerializeField]
    private Transform _target;

    public override ISensor[] CreateSensors()
    {
        return new ISensor[] { new SorterPositionalSensor(_agent, _target, 4, "Position") };
    }

    public override int SourcesCount => 1;

    public override ObservationSpec ObservationSpec => ObservationSpec.Vector(4);
}

public class SorterPositionalSensor : ISensor
{
    private Agent _agent;
    private Transform _target;
    private VectorSensor _sensor;

    private ObservationSpec _observationSpec;
    private string _name;

    public SorterPositionalSensor(Agent agent, Transform target, int observationSize, string name = null,
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
        _sensor.AddObservation((_agent.transform.position.x - _target.transform.position.x) / 20f);
        _sensor.AddObservation((_agent.transform.position.z - _target.transform.position.z) / 20f);

        _sensor.AddObservation(_agent.transform.forward.x);
        _sensor.AddObservation(_agent.transform.forward.z);

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