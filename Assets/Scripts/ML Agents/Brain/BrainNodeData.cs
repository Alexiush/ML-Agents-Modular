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

            var defaultOutput = new DynamicTensorShape(-1);

            var outputShapes = compilationContext
                .GetOutputNodes(this)
                .SelectMany(n =>
                {
                    if (n is IShapeRequestor shapeRequestor)
                    {
                        return shapeRequestor.GetRequestedShape();
                    }

                    return new List<DynamicTensorShape>() { defaultOutput };
                })
                .ToList();

            var input = $"torch.cat([{string.Join(", ", inputReferences.Select(r => $"torch.flatten({r}, start_dim=1)"))}], dim=1)";
            return Brain.Switch.Layer.Compile(
                compilationContext,
                inputShapes, outputShapes,
                _inputSymbolicShapes,
                _outputSymbolicShapes,
                string.Join(", ", input)
            );
        }

        public override string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            int index = compilationContext
                .GetOutputNodes(this)
                .IndexOf(outputReceiver);

            return $"[{index}]";
        }

        public override List<SymbolicTensorDim> GetInputSymbolicShapes(IConnectionsContext connectionsContext)
        {
            return new List<SymbolicTensorDim>();
        }

        public override List<SymbolicTensorDim> GetOutputSymbolicShapes(IConnectionsContext connectionsContext)
        {
            var inputShapes = connectionsContext
                .GetInputNodes(this)
                .SelectMany(n => n.GetOutputShape(connectionsContext))
                .ToList();

            var defaultOutput = new DefinedSymbolicTensorDim(1);

            return connectionsContext
                .GetOutputNodes(this)
                .SelectMany(n =>
                {
                    if (n is IShapeRequestor shapeRequestor)
                    {
                        return n.GetInputSymbolicShapes(connectionsContext);
                    }

                    return new List<SymbolicTensorDim>() { defaultOutput };
                })
                .ToList();
        }

        public override List<DynamicTensorShape> GetOutputShape(IConnectionsContext connectionsContext)
        {
            var inputShapes = connectionsContext
                .GetInputNodes(this)
                .SelectMany(n => n.GetOutputShape(connectionsContext))
                .ToList();

            var defaultOutput = new DynamicTensorShape(-1);

            return connectionsContext
                .GetOutputNodes(this)
                .SelectMany(n =>
                {
                    if (n is IShapeRequestor shapeRequestor)
                    {
                        return shapeRequestor.GetRequestedShape();
                    }

                    return new List<DynamicTensorShape>() { defaultOutput };
                })
                .ToList();
        }

        public override List<DynamicTensorShape> GetPartialOutputShape(IConnectionsContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            if (outputReceiver is IShapeRequestor shapeRequestor)
            {
                return shapeRequestor.GetRequestedShape();
            }

            var inputShapes = compilationContext
                .GetInputNodes(this)
                .SelectMany(n => n.GetOutputShape(compilationContext))
                .ToList();

            var defaultOutput = new DynamicTensorShape(-1);
            return new List<DynamicTensorShape>() { defaultOutput };
        }
    }
}
