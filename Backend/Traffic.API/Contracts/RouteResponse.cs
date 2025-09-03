namespace Traffic.API.Contracts
{
    public record RouteResponse(
        Guid Id,
        Guid TransportId,
        TransportResponse Transport,
        DateTime RouteTime,
        List<PointResponse> Points);
}
