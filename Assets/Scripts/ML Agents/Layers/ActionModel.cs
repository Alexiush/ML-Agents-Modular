using ModularMLAgents.Actuators;
using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using ModularMLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Actuators;
using Unity.Sentis;
using UnityEngine;

namespace ModularMLAgents.Layers
{
    [System.Serializable]
    public class ActionModel : LayerBase, IEncoder, IDecoder
    {
        public ActionSpec ActionSpec;

        public override List<SymbolicTensorDim> SymbolicForwardPass(List<SymbolicTensorDim> dims)
        {
            throw new System.NotImplementedException();
        }

        public override List<SymbolicTensorDim> SymbolicBackwardPass(List<SymbolicTensorDim> dims)
        {
            throw new System.NotImplementedException();
        }

        public override string Compile(CompilationContext compilationContext,
            List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes,
            List<SymbolicTensorDim> inputDims, List<SymbolicTensorDim> outputDims,
            string input)
        {
            compilationContext.AddDependencies("mlagents.trainers.torch_entities.action_model", "ActionModel");
            return $"ActionModel({inputShapes.First()}, ActionSpec({ActionSpec.NumContinuousActions}, ({string.Join(", ", ActionSpec.BranchSizes)})))";
        }

        public override List<DynamicTensorShape> GetShape(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
        {
            throw new System.NotImplementedException();
        }

        public override bool Validate(List<DynamicTensorShape> inputShapes, List<DynamicTensorShape> outputShapes)
        {
            return inputShapes.Count == 1 && inputShapes[0].rank == 1;
        }
    }
}
