using System.Linq;
using UnityEngine;

[System.Serializable]
public class SensorNodeData : AgentGraphNodeData
{
    [SerializeField] public Sensor Sensor;

    public override AgentGraphNode Load()
    {
        var sensorNode = new SensorNode(Metadata);
        sensorNode.SetPosition(Metadata.Position);
        Ports.ForEach(p => p.Instantiate(sensorNode));

        return sensorNode;
    }

    public override string GetExpressionBody(CompilationContext compilationContext)
    {
        var input = compilationContext.GetInputs(this).First();
        return Sensor.Encoder.Compile(compilationContext, input);
    }
}