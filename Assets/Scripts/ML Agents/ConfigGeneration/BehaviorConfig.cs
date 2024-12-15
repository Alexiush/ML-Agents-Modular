using ModularMLAgents.Utilities;
using UnityEngine;

namespace ModularMLAgents.Configuration
{
    public class BehaviorConfig : MonoBehaviour
    {
        [field: SerializeField]
        public BehaviorName BehaviorName { get; private set; }
        [field: SerializeField]
        public Behavior Behavior { get; private set; }
    }
}
