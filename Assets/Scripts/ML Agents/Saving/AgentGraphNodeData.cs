using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

[System.Serializable]
public abstract class AgentGraphNodeData : ScriptableObject
{
    [SerializeField] public AgentGraphElementMetadata Metadata;
    [SerializeField] public List<AgentGraphPortData> Ports;

    public abstract AgentGraphNode Load();
}

[System.Serializable]
public class AgentGraphPortData
{
    public Orientation Orientation;
    public Direction Direction;
    public Port.Capacity Capacity;
    public System.Type Type;

    public string Name;
    public string GUID;

    public AgentGraphPortData(Port port)
    {
        Orientation = port.orientation;
        Direction = port.direction;
        Capacity = port.capacity;
        Type = port.portType;

        Name = port.name;
        GUID = port.viewDataKey;
    }

    public void Instantiate(Node node)
    {
        var port = node.InstantiatePort(Orientation, Direction, Capacity, Type);
        port.name = Name;
        port.viewDataKey = GUID;
    }

}
