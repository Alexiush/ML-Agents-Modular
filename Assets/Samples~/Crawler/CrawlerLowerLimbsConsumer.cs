using ModularMLAgents.Components;
using System.Linq;
using Unity.MLAgents.Actuators;
using UnityEditor;
using UnityEngine;

public class CrawlerLowerLimbsConsumer : ConsumerProvider
{
    [SerializeField]
    private CrawlerAgent _agent;

    public override IActuator[] CreateActuators()
    {
        return _agent.BodyParts
            .Select((t, i) => (t, i))
            .Where((t, i) => i % 2 == 1)
            .Select(ti => new CrawlerLimbActuator(_agent, ti.t, false, "CrawlerLimbActuator_" + GUID.Generate().ToString()))
            .ToArray();
    }

    public override int ConsumersCount => _agent.LimbsCount;

    public override ActionSpec ActionSpec => ActionSpec.MakeContinuous(2);
}
