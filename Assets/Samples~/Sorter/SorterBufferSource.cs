using ModularMLAgents.Components;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SorterBufferSource : SourceProvider
{
    [SerializeField]
    private SorterAgent _agent;

    public override ISensor[] CreateSensors()
    {
        return new ISensor[] { new SorterBufferSensor(_agent, 20, "TargetsSource") };
    }

    public override int SourcesCount => 1;

    public override ObservationSpec ObservationSpec => new ObservationSpec(
        new InplaceArray<int>(20, 23),
        new InplaceArray<DimensionProperty>(DimensionProperty.VariableSize, DimensionProperty.None)
    );
}

public class SorterBufferSensor : ISensor
{
    private SorterAgent _agent;
    private BufferSensor _sensor;

    private ObservationSpec _observationSpec;
    private string _name;

    public SorterBufferSensor(SorterAgent agent, int observationSize, string name = null,
        ObservationType observationType = ObservationType.Default)
    {
        _agent = agent;
        _sensor = new BufferSensor(observationSize, 23, name + "_vector");

        _name = name;
        _observationSpec = new ObservationSpec(
            new InplaceArray<int>(20, 23),
            new InplaceArray<DimensionProperty>(DimensionProperty.VariableSize, DimensionProperty.None)
        );
    }

    public int Write(ObservationWriter writer)
    {
        foreach (var item in _agent.CurrentlyVisibleTilesList)
        {
            // Each observation / tile in the BufferSensor will have 22 values
            // The first 20 are one hot encoding of the value of the tile
            // The 21st and 22nd are the position of the tile relative to the agent
            // The 23rd is a boolean : 1 if the tile was visited already and 0 otherwise
            float[] listObservation = new float[23];
            listObservation[item.NumberValue] = 1.0f;
            var tileTransform = item.transform.GetChild(1);
            listObservation[20] = (tileTransform.position.x - _agent.transform.position.x) / 20f;
            listObservation[21] = (tileTransform.position.z - _agent.transform.position.z) / 20f;
            listObservation[22] = item.IsVisited ? 1.0f : 0.0f;
            // Here, the observation for the tile is added to the BufferSensor
            _sensor.AppendObservation(listObservation);
        }

        return _sensor.Write(writer);
    }

    public ObservationSpec GetObservationSpec()
    {
        return _observationSpec;
    }

    public void Update()
    {
        // In this framework I suggest not clearing the observations
        _sensor.Update();
    }

    public void Reset()
    {
        _sensor.Reset();
    }

    public string GetName()
    {
        return _name;
    }

    public virtual byte[] GetCompressedObservation()
    {
        return null;
    }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }
}
