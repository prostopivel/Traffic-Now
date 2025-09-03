namespace Transport.Core.Abstractions
{
    public interface ITransportRepository
    {
        void CreateTransport(IDataService _dataService);
        Core.Models.Transport GetTransport();
    }
}