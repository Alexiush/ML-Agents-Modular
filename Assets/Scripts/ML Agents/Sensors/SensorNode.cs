using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
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

    public SensorNode() : base() 
    {
        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Schema));
        inputPort.name = "Input signals";

        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Schema));
        outputPort.name = "Output signals";
    }

    public SensorNode(AgentGraphElementMetadata metadata) : base()
    {
        viewDataKey = metadata.GUID;
        Metadata = metadata;
    }

    public override void Draw()
    {
        titleContainer.Q<Label>("title-label").text = "Sensor";

        foreach (var port in Ports.Where(p => p.direction == Direction.Input))
        {
            inputContainer.Add(port);
        }

        foreach (var port in Ports.Where(p => p.direction == Direction.Output))
        {
            outputContainer.Add(port);
        }

        extensionContainer.Add(_sensor.Encoder.GetVisualElement());
        RefreshExpandedState();
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
        data.Ports = Ports.Select(p => new AgentGraphPortData(p)).ToList();
        data.Sensor = _sensor;

        return data;
    }
}
