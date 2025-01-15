using MLAgents.Configuration;

namespace ModularMLAgents.Trainers
{
    public interface ICustomTrainer : ITrainer
    {
        public CustomHyperparameters CustomHyperparameters { get; }
    }
}
