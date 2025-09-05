namespace Traffic.Core.Abstractions.Services
{
    public interface IMapSerializeService
    {
        public Task<Guid> CreateMapJson(string path, Guid mapId);
    }
}
