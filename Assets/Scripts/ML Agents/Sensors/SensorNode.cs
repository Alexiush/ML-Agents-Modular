using UnityEditor;
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
    private Sensor _sensor = new Sensor();

    public SensorNode() : base() { }

    public SensorNode(AgentGraphElementMetadata metadata) : base()
    {
        Metadata = metadata;
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

    public override AgentGraphElementMetadata GetMetadata()
    {
        return new AgentGraphElementMetadata()
        {
            Position = this.GetPosition()
        };
    }

    public override AgentGraphNodeData Save(UnityEngine.Object parent)
    {
        var data = Metadata.Asset as SensorNodeData;
        if (data is null)
        {
            data = ScriptableObject.CreateInstance<SensorNodeData>();
            AssetDatabase.AddObjectToAsset(data, parent);
            Metadata.Asset = data;
        }

        data.Metadata = Metadata;
        data.Sensor = _sensor;

        return data;
    }
}
