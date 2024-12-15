using UnityEngine;

namespace ModularMLAgents.Configuration
{
    public class EnvironmentConfig : MonoBehaviour
    {
        [field: SerializeField]
        public Config Configuration { get; private set; }
    }
}