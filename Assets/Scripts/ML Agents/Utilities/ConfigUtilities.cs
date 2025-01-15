using MLAgents.Configuration;
using ModularMLAgents.Models;
using ModularMLAgents.Trainers;

namespace ModularMLAgents.Utilities
{
    public static class ConfigUtilities
    {
        public static void BindBehaviorToAgentGraph(Behavior behavior, AgentGraphData graphData)
        {
            if (behavior.Trainer is ICustomTrainer customTrainer)
            {
                customTrainer.CustomHyperparameters.PathToModel = $"{System.IO.Path.Combine(graphData.ModelDirectory, graphData.name)}.py";
                customTrainer.CustomHyperparameters.PathToMapping = $"{System.IO.Path.Combine(graphData.ModelDirectory, behavior.BehaviorId)}.py";
            }
        }
    }
}