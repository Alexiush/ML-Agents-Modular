using UnityEngine.UIElements;

[System.Serializable]
public class Encoder
{
    // Base class for encoding architecture, passing raw data to the brain is not the best idea
    // It also won't allow to merge different data, so it needs to be encoded into the latent space

    public EncoderType Type;

    public VisualElement GetVisualElement()
    {
        // Should instantiate required ui parts and subscribe them to Encoder data

        var encoderContainer = new VisualElement();

        var typeDropdown = new EnumField(Type);
        encoderContainer.Add(typeDropdown);

        return encoderContainer;
    }
}

public enum EncoderType
{
    Linear,
    Convolution
}
