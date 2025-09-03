using Transport.Core.Models;

namespace Transport.Core.Abstractions
{
    public interface IDataService
    {
        Map Map { get; set; }
        Models.Transport Transport { get; set; }
    }
}