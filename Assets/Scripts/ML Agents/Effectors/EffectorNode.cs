public class EffectorNode
{
    // Effector node is the one that performs action based on the brain's output
    // Here effector is a contract about the shape and type of data before it goes to consumer - routine that applies the effector

    public Schema InputSchema;
    public Decoder Decoder;
}
