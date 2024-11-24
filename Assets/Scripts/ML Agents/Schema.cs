using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;

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

    public InplaceArray<int> ToShape()
    {
        var shape = InplaceArray<int>.FromList(Dimensions.Select(d => (int)d).ToList());
        return shape;
    }
}
