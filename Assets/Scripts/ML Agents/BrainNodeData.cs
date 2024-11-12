using UnityEngine;

[System.Serializable]
public class BrainNodeData : AgentGraphNodeData
{
    [SerializeField] public Brain Brain;

    public override AgentGraphNode Load()
    {
        var brainNode = new BrainNode(Metadata);
        brainNode.SetPosition(Metadata.Position);

        return brainNode;
    }
}
