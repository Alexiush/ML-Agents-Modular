using ModularMLAgents.Configuration;

namespace ModularMLAgents.Trainers
{
    public interface ITrainer
    {
        public string TrainerType { get; }
        public Hyperparameters Hyperparameters { get; }
    }

    public interface ICustomTrainer : ITrainer
    {
        public CustomHyperparameters CustomHyperparameters { get; }
    }
}
