using UnityEngine;

[System.Serializable]
public class ConsumerNodeData : AgentGraphNodeData
{
    [SerializeField] public Consumer Consumer;

    public override AgentGraphNode Load()
    {
        var consumerNode = new ConsumerNode(Metadata);
        consumerNode.SetPosition(Metadata.Position);
        Ports.ForEach(p => p.Instantiate(consumerNode));

        return consumerNode;
    }
}
