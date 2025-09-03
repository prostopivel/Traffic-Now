namespace Traffic.API.Contracts
{
    public record RouteRequest(
        Guid TransportId,
        DateTime RouteTime);
}
