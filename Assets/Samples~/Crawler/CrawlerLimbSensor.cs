using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples.Crawler;
using UnityEngine;

public class CrawlerLimbSensor : ISensor
{
    private CrawlerAgent _agent;
    private Transform _bpTransform;
    private JointDriveController _jdController;
    private VectorSensor _sensor;

    private ObservationSpec _observationSpec;
    private string _name;

    public CrawlerLimbSensor(CrawlerAgent agent, Transform transform, int observationSize, string name = null,
        ObservationType observationType = ObservationType.Default)
    {
        _agent = agent;
        _jdController = _agent.JdController;
        // Manager sets transform default position
        _bpTransform = transform;
        _sensor = new VectorSensor(observationSize, name + "_vector", observationType);

        _name = name;
        _observationSpec = ObservationSpec.Vector(observationSize, observationType);
    }

    public int Write(ObservationWriter writer)
    {
        var bp = _jdController.bodyPartsDict[_bpTransform];
        _sensor.AddObservation(bp.groundContact.touchingGround); // Is this bp touching the ground
        _sensor.AddObservation(bp.currentStrength / _jdController.maxJointForceLimit);

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
