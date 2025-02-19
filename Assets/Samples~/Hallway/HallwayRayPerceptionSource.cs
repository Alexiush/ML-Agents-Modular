using ModularMLAgents.Components;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HallwayRayPerceptionSource : SourceProvider
{
    [SerializeField]
    private RayPerceptionSensorComponent3D _rayPerceptionSensor;

    public override ISensor[] CreateSensors() => _rayPerceptionSensor.CreateSensors();

    public override int SourcesCount => 1;

    public override ObservationSpec ObservationSpec => _rayPerceptionSensor.CreateSensors()[0].GetObservationSpec();
}
