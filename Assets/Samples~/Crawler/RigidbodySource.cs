using ModularMLAgents.Components;
using System.Linq;
using Unity.MLAgents.Extensions.Sensors;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RigidbodySource : SourceProvider
{
    [SerializeField]
    private CrawlerAgent _agent;

    [SerializeField]
    private RigidBodySensorComponent sensorComponent;

    public override ISensor[] CreateSensors()
    {
        return sensorComponent.CreateSensors();
    }

    public override int SourcesCount => 1;

    public override ObservationSpec ObservationSpec => CreateSensors().First().GetObservationSpec();
}
