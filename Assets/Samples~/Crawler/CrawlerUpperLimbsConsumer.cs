using ModularMLAgents.Components;
using System.Linq;
using Unity.MLAgents.Actuators;
using UnityEditor;
using UnityEngine;

public class CrawlerUpperLimbsConsumer : ConsumerProvider
{
    [SerializeField]
    private CrawlerAgent _agent;

    public override IActuator[] CreateActuators()
    {
        return _agent.BodyParts
            .Select((t, i) => (t, i))
            .Where((t, i) => i % 2 == 0)
            .Select(ti => new CrawlerLimbActuator(_agent, ti.t, true, "CrawlerLimbActuator_" + GUID.Generate().ToString()))
            .ToArray();
    }

    public override int ConsumersCount => _agent.LimbsCount;

    public override ActionSpec ActionSpec => ActionSpec.MakeContinuous(3);
}
