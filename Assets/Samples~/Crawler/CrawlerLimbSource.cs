using ModularMLAgents.Components;
using System.Linq;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;

public class CrawlerLimbSource : SourceProvider
{
    [SerializeField]
    private CrawlerAgent _agent;

    public override ISensor[] CreateSensors()
    {
        return _agent.BodyParts
            .Select(t => new CrawlerLimbSensor(_agent, t, 2, "SourceLimb_" + GUID.Generate().ToString()))
            .ToArray();
    }

    public override int SourcesCount => _agent.LimbsCount * 2;

    public override ObservationSpec ObservationSpec => ObservationSpec.Vector(2);
}
