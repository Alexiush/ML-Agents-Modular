using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Sensor
{
    // Sensor node passes data to the brain for it to be processed
    // Here sensor is a contract about the shape and type of data that is passed from the source

    public Schema InputSchema = new Schema();
    public Encoder Encoder = new Encoder();
}

[NodePath("Sensor")]
public class SensorNode : AgentGraphNode
{
    private Sensor _sensor;

    public SensorNode()
    {
        _sensor = new Sensor();
    }

    public override void Draw()
    {
        titleContainer.Q<Label>("title-label").text = "Sensor";

        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Schema));
        inputPort.name = "Input signals";
        inputContainer.Add(inputPort);

        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Schema));
        outputPort.name = "Output signals";
        outputContainer.Add(outputPort);

        extensionContainer.Add(_sensor.Encoder.GetVisualElement());
    }
}
