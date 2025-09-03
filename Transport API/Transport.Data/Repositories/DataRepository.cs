using Transport.Core.Abstractions;
using Transport.Core.Models;

namespace Transport.Data.Repositories
{
    public class DataRepository : IDataRepository
    {
        public Map Map { get; set; }

        public Core.Models.Transport Transport { get; set; }
    }
}
