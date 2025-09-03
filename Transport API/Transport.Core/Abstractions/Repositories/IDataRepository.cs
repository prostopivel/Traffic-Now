using Transport.Core.Models;

namespace Transport.Core.Abstractions
{
    public interface IDataRepository
    {
        Map Map { get; set; }
        Core.Models.Transport Transport { get; set; }
    }
}