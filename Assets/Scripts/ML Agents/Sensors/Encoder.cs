using UnityEngine.UIElements;

[System.Serializable]
public class Encoder
{
    // Base class for encoding architecture, passing raw data to the brain is not the best idea
    // It also won't allow to merge different data, so it needs to be encoded into the latent space

    public EncoderType Type;
}

public enum EncoderType
{
    Linear,
    Convolution
}
