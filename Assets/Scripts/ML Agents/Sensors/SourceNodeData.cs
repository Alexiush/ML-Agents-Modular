using UnityEngine;
using ModularMLAgents.Compilation;
using Unity.Sentis;
using ModularMLAgents.Models;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public class SourceNodeData : AgentGraphNodeData
    {
        [SerializeField]
        public Source Source;

        public override AgentGraphNode Load(AgentGraphContext context)
        {
            var sourceNode = new SourceNode(context, Metadata);
            sourceNode.SetPosition(Metadata.Position);
            Ports.ForEach(p => p.Instantiate(sourceNode));

            return sourceNode;
        }

        public override string GetExpressionBody(CompilationContext compilationContext)
        {
            // Input variable is predefined
            var input = $"input_tensor[{compilationContext.GetSourceNumber(this)}]";

            // Source should be taking its tensor by index
            return input;
        }

        public override TensorShape GetShape(CompilationContext compilationContext)
        {
            return Source.OutputShape.ToShape();
        }
    }
}
