using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEditor;

[System.Serializable]
public record Schema
{
    // Schema defines dimensions and type of the data
    public List<uint> Dimensions = new List<uint>();

    public TensorShape AsTensorShape()
    {
        var shape = new System.ReadOnlySpan<int>(Dimensions.Take(8).Select(d => (int)d).ToArray());
        return new TensorShape(shape);
    }
}
