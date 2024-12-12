using ModularMLAgents.Layers;

namespace ModularMLAgents.Actuators
{
    public interface IDecoder : ILayer
    {
        // Brain does not need to deal with the final output of effector, only match the signal that goes back
        // Thus decoder matches reduced input with the output
    }
}
