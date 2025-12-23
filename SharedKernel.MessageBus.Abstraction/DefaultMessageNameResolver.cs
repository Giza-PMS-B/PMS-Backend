using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedKernel.MessageBus.Abstraction
{
    public class DefaultMessageNameResolver : IMessageNameResolver
    {
        public string Resolve<T>()
        {
            var name = typeof(T).Name;
            name = Regex.Replace(name, "Event$", "");
            name = Regex.Replace(name, "([a-z])([A-Z])", "$1-$2");
            return name.ToLowerInvariant();
        }
    }
}
