using VYaml.Annotations;

namespace ModularMLAgents.Configuration
{
    [YamlObject]
    [System.Serializable]
    public partial class NetworkSettings
    {
        public int HiddenUnits = 128;
        public int NumLayers = 2;
        public bool Normalize = false;
        public string VisualEncodeType = "simple";
        public string ConditioningType = "hyper";
    }
}
