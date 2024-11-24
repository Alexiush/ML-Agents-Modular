using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
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
        var input = compilationContext.GetInputNodes(this).First();
        var inputShape = input.GetShape(compilationContext);
        var inputReference = compilationContext.GetReference(input);

        return Sensor.Encoder.Compile(compilationContext, inputShape, inputReference);
    }

    public override InplaceArray<int> GetShape(CompilationContext compilationContext)
    {
        var input = compilationContext.GetInputNodes(this).First();
        var inputShape = input.GetShape(compilationContext);

        return Sensor.Encoder.GetShape(inputShape);
    }
}