using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using UnityEngine;

public class CrawlerBodySensor : ISensor
{
    private CrawlerAgent _agent;
    private Transform _target;
    private OrientationCubeController _orientationCube;
    private VectorSensor _sensor;

    private ObservationSpec _observationSpec;
    private string _name;

    public CrawlerBodySensor(CrawlerAgent agent, int observationSize, string name = null,
        ObservationType observationType = ObservationType.Default)
    {
        _agent = agent;
        _orientationCube = _agent.OrientationCube;
        _target = _agent.Target;
        _sensor = new VectorSensor(observationSize, name + "_vector", observationType);

        _name = name;
        _observationSpec = ObservationSpec.Vector(observationSize, observationType);
    }

    public int Write(ObservationWriter writer)
    {
        var cubeForward = _orientationCube.transform.forward;

        //velocity we want to match
        var velGoal = cubeForward * _agent.TargetWalkingSpeed;
        //ragdoll's avg vel
        var avgVel = _agent.GetAvgVelocity();

        //current ragdoll velocity. normalized
        _sensor.AddObservation(Vector3.Distance(velGoal, avgVel));
        //avg body vel relative to cube
        _sensor.AddObservation(_orientationCube.transform.InverseTransformDirection(avgVel));
        //vel goal relative to cube
        _sensor.AddObservation(_orientationCube.transform.InverseTransformDirection(velGoal));
        //rotation delta
        _sensor.AddObservation(Quaternion.FromToRotation(_agent.Body.forward, cubeForward));

        //Add pos of target relative to orientation cube
        _sensor.AddObservation(_orientationCube.transform.InverseTransformPoint(_target.transform.position));

        RaycastHit hit;
        float maxRaycastDist = 10;
        if (Physics.Raycast(_agent.Body.position, Vector3.down, out hit, maxRaycastDist))
        {
            _sensor.AddObservation(hit.distance / maxRaycastDist);
        }
        else
        {
            _sensor.AddObservation(1);
        }

        _sensor.AddObservation(_target.localPosition);
        _sensor.AddObservation(_agent.transform.localPosition);

        _sensor.AddObservation(_agent.JdController.bodyPartsDict[_agent.Body].groundContact.touchingGround);

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
