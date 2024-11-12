using UnityEngine;

[System.Serializable]
public class SensorNodeData : AgentGraphNodeData
{
    [SerializeField] public Sensor Sensor;

    public override AgentGraphNode Load()
    {
        var sensorNode = new SensorNode(Metadata);
        sensorNode.SetPosition(Metadata.Position);

        return sensorNode;
    }
}