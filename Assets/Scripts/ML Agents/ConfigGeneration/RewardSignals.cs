using VYaml.Annotations;

namespace ModularMLAgents.Configuration
{
    [YamlObject]
    [System.Serializable]
    public partial class ExtrinsicSignals
    {
        public float Strength = 1.0f;
        public float Gamma = 0.99f;
    }

    [YamlObject]
    [System.Serializable]
    public partial class RewardSignals
    {
        public ExtrinsicSignals Extrinsic = new ExtrinsicSignals();
    }
}
