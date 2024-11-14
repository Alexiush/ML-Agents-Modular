using UnityEditor;
using UnityEngine;

public class AgentGraphOpener : UnityEditor.AssetModificationProcessor
{
    [UnityEditor.Callbacks.OnOpenAsset(0)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Object asset = EditorUtility.InstanceIDToObject(instanceID);

        if (asset is AgentGraphData graphData)
        {
            AgentGraph.OpenGraph(graphData);
            return true;
        }

        return false;
    }
}