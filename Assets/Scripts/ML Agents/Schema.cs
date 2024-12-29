using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEditor;

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

    public DataType DataType = DataType.float32;
    public List<uint> Dimensions = new List<uint>();

    public TensorShape AsTensorShape()
    {
        var shape = new System.ReadOnlySpan<int>(Dimensions.Take(8).Select(d => (int)d).ToArray());
        return new TensorShape(shape);
    }
}
