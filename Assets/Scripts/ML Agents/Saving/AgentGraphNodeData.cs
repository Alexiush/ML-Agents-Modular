using UnityEngine;

[System.Serializable]
public abstract class AgentGraphNodeData : ScriptableObject
{
    [SerializeField] public AgentGraphElementMetadata Metadata;

    public abstract AgentGraphNode Load();
}
