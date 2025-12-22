using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SharedKernel.MessageBus.Abstraction
{
    public sealed class JsonMessageSerializer : IMessageSerializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        public byte[] Serialize<T>(T value)
        {
            var json = JsonConvert.SerializeObject(value, Settings);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, Settings)!;
        }
    }
}
