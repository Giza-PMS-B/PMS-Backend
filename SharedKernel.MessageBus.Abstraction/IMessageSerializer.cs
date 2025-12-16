namespace SharedKernel.MessageBus.Abstraction
{
    public interface IMessageSerializer
    {
        byte[] Serialize<T>(T value);
        T Deserialize<T>(byte[] bytes);
    }
}

