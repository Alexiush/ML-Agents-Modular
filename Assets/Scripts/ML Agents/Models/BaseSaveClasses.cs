using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ModularMLAgents.Models
{
    [System.Serializable]
    public struct AgentGraphElementMetadata
    {
        public string GUID;
        public Rect Position;
        public UnityEngine.Object Asset;
    }

    [System.Serializable]
    public class AgentGraphEdgeData
    {
        public string InputGUID;
        public string OutputGUID;

        public AgentGraphEdgeData(Edge edge)
        {
            InputGUID = edge.input.viewDataKey;
            OutputGUID = edge.output.viewDataKey;
        }
    }
}
