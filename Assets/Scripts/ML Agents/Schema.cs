using ModularMLAgents.Actuators;
using ModularMLAgents.Models;
using ModularMLAgents.Sensors;
using ModularMLAgents;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public enum DataType
{
    uint8,
    uint16,
    int32,
    float8,
    float16,
    float32,
    float64,
    boolean
}

[System.Serializable]
public record Schema
{
    // Schema defines dimensions and type of the data

    public DataType DataType;
    public List<uint> Dimensions;

    public TensorShape ToShape()
    {
        var shape = new System.ReadOnlySpan<int>(Dimensions.Select(d => (int)d).ToArray());
        return new TensorShape(shape);
    }
}
