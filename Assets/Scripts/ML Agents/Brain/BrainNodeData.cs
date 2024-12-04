using System.Linq;
using UnityEngine;
using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using Unity.Sentis;

namespace ModularMLAgents.Brain
{
    [System.Serializable]
    public class BrainNodeData : AgentGraphNodeData
    {
        [SerializeField] public Brain Brain;

        public override AgentGraphNode Load(AgentGraphContext context)
        {
            var brainNode = new BrainNode(context, Metadata);
            brainNode.SetPosition(Metadata.Position);
            Ports.ForEach(p => p.Instantiate(brainNode));

            return brainNode;
        }

        public override string GetExpressionBody(CompilationContext compilationContext)
        {
            var inputs = compilationContext
                .GetInputNodes(this);

            var inputReferences = inputs
                .Select(i => compilationContext.GetReference(i));

            var inputShapeMerged = inputs
                .Select(n => n.GetShape(compilationContext)[0])
                .Sum();

            var outputs = compilationContext
                .GetOutputNodes(this)
                .Select(n => n.GetShape(compilationContext))
                .ToList();

            var input = $"torch.cat([{string.Join(", ", inputReferences)}], dim = 1)";
            return Brain.Switch.Compile(compilationContext, new TensorShape(inputShapeMerged), outputs, string.Join(", ", input));
        }

        public override TensorShape GetShape(CompilationContext compilationContext)
        {
            // For now brain is linear-only
            return new TensorShape(128);
        }
    }
}
