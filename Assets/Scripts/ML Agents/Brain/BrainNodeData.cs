using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;

namespace ModularMLAgents.Brain
{
    [System.Serializable]
    public class BrainNodeData : AgentGraphNodeData
    {
        [SerializeField] public Brain Brain;

        public override IAgentGraphNode Load(AgentGraphContext context)
        {
            var brainNode = new BrainNode(context, this);
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

            var inputShapes = inputs
                .SelectMany(n => n.GetOutputShape(compilationContext))
                .ToList();

            var defaultOutput = new TensorShape(inputShapes.Select(s => s[0]).Sum());

            var outputs = compilationContext
                .GetOutputNodes(this)
                .SelectMany(n =>
                {
                    if (n is IShapeRequestor shapeRequestor)
                    {
                        return shapeRequestor.GetRequestedShape();
                    }

                    return new List<TensorShape>() { defaultOutput };
                })
                .ToList();

            var input = $"torch.cat([{string.Join(", ", inputReferences)}], dim = 1)";
            return Brain.Switch.Layer.Compile(compilationContext, inputShapes, outputs, string.Join(", ", input));
        }

        public override string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            int index = compilationContext
                .GetOutputNodes(this)
                .IndexOf(outputReceiver);

            return $"[{index}]";
        }

        public override List<TensorShape> GetOutputShape(IConnectionsContext compilationContext)
        {
            // For now brain is linear-only
            var inputShapes = compilationContext
                .GetInputNodes(this)
                .SelectMany(n => n.GetOutputShape(compilationContext))
                .ToList();

            var defaultOutput = new TensorShape(inputShapes.Select(s => s[0]).Sum());

            return compilationContext
                .GetOutputNodes(this)
                .SelectMany(n =>
                {
                    if (n is IShapeRequestor shapeRequestor)
                    {
                        return shapeRequestor.GetRequestedShape();
                    }

                    return new List<TensorShape>() { defaultOutput };
                })
                .ToList();
        }

        public override List<TensorShape> GetPartialOutputShape(IConnectionsContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            if (outputReceiver is IShapeRequestor shapeRequestor)
            {
                return shapeRequestor.GetRequestedShape();
            }

            var inputShapes = compilationContext
                .GetInputNodes(this)
                .SelectMany(n => n.GetOutputShape(compilationContext))
                .ToList();

            var defaultOutput = new TensorShape(inputShapes.Select(s => s[0]).Sum());
            return new List<TensorShape>() { defaultOutput };
        }
    }
}
