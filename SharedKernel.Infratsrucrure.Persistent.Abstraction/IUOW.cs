using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Infrastructure.Persistent.Abstraction
{
    public interface IUOW
    {
        public Task SaveChangesAsync();
    }
}
