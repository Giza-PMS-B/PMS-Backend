namespace SharedKernel.MessageBus.Abstraction
{
    public interface IMessageNameResolver
    {
        string Resolve<T>();
        string Resolve(Type type);

    }
}
