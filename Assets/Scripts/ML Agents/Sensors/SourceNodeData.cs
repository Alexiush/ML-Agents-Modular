using UnityEngine;

[System.Serializable]
public class SourceNodeData : AgentGraphNodeData
{
    [SerializeField] public InputSource Source;

    public override AgentGraphNode Load()
    {
        var sourceNode = new SourceNode(Metadata);
        sourceNode.SetPosition(Metadata.Position);
        Ports.ForEach(p => p.Instantiate(sourceNode));

        return sourceNode;
    }
}
