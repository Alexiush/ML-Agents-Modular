using UnityEngine;

[System.Serializable]
public class ActuatorNodeData : AgentGraphNodeData
{
    [SerializeField] public Actuator Actuator;

    public override AgentGraphNode Load()
    {
        var actuatorNode = new ActuatorNode(Metadata);
        actuatorNode.SetPosition(Metadata.Position);
        Ports.ForEach(p => p.Instantiate(actuatorNode));

        return actuatorNode;
    }
}
