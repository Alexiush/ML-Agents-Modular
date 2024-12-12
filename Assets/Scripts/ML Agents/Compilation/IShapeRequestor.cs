using System.Collections.Generic;
using Unity.Sentis;

namespace ModularMLAgents.Compilation
{
    public interface IShapeRequestor
    {
        public List<TensorShape> GetRequestedShape();
    }
}
