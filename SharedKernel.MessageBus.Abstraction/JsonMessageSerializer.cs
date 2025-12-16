using System.Text.Json;

namespace SharedKernel.MessageBus.Abstraction
{
    public sealed class JsonMessageSerializer : IMessageSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public byte[] Serialize<T>(T value)
            => JsonSerializer.SerializeToUtf8Bytes(value, Options);

        public T Deserialize<T>(byte[] bytes)
            => JsonSerializer.Deserialize<T>(bytes, Options)!;
    }
}
