using ModularMLAgents.Components;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RollerSourceSpeed : SourceProvider
{
    [SerializeField]
    private Agent _agent;
    [SerializeField]
    private Transform _target;

    public override ISensor[] CreateSensors()
    {
        return new ISensor[] { new RollerSpeedSensor(_agent, 2, "SourceSpeed") };
    }

    public override int SourcesCount => 1;

    public override ObservationSpec ObservationSpec => ObservationSpec.Vector(2);
}

public class RollerSpeedSensor : ISensor
{
    private Agent _agent;
    private Rigidbody _rigidbody;
    private VectorSensor _sensor;

    private ObservationSpec _observationSpec;
    private string _name;

    public RollerSpeedSensor(Agent agent, int observationSize, string name = null,
        ObservationType observationType = ObservationType.Default)
    {
        _agent = agent;
        _rigidbody = agent.GetComponent<Rigidbody>();
        _sensor = new VectorSensor(observationSize, name + "_vector", observationType);

        _name = name;
        _observationSpec = ObservationSpec.Vector(observationSize, observationType);
    }

    public int Write(ObservationWriter writer)
    {
        // _sensor.Reset();
        _sensor.AddObservation(_rigidbody.velocity.x);
        _sensor.AddObservation(_rigidbody.velocity.z);

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
