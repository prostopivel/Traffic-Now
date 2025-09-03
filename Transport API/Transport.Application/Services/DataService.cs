using Transport.Core.Abstractions;
using Transport.Core.Models;

namespace Transport.Application.Services
{
    public class DataService : IDataService
    {
        private readonly IDataRepository _dataRepository;

        public DataService(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public Map Map
        {
            get => _dataRepository.Map;
            set => _dataRepository.Map = value;
        }

        public Core.Models.Transport Transport
        {
            get => _dataRepository.Transport;
            set => _dataRepository.Transport = value;
        }
    }
}
