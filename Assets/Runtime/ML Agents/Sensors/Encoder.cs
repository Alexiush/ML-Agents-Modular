using ModularMLAgents.Layers;

namespace ModularMLAgents.Sensors
{
    public interface IEncoder : ILayer
    {
        // Tag interface for encoding, passing raw data to the brain is not the best idea
        // It also won't allow to merge different data, so it needs to be encoded into the latent space
    }
}
