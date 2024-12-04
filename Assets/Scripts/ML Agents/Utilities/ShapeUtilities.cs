using System;
using System.Linq;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Sentis;

namespace ModularMLAgents.Utilities
{
    public static class ShapeUtilities
    {
        public static bool CompareActionSpecs(ActionSpec first, ActionSpec second)
        {
            bool sameContinuous = first.NumContinuousActions == second.NumContinuousActions;
            bool sameDiscrete = first.BranchSizes.SequenceEqual(second.BranchSizes);

            return sameContinuous && sameDiscrete;
        }

        public static TensorShape ObservationsAsTensor(ObservationSpec observationSpec)
        {
            var shape = observationSpec.Shape;
            var dimensions = new int[shape.Length];

            for (int dimension = 0; dimension < shape.Length; dimension++)
            {
                dimensions[dimension] = shape[dimension];
            }

            var dimensionsSpan = new ReadOnlySpan<int>(dimensions);
            return new TensorShape(dimensionsSpan);
        }
    }
}
