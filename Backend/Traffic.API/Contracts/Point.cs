namespace Traffic.API.Contracts
{
    public record PointResponse(
        Guid Id,
        Guid MapId,
        double X,
        double Y,
        string Name,
        List<Guid> ConnectedPointsIds);

    public record PointTransportResponse(
        Guid TransportId,
        Guid MapId,
        double X,
        double Y);

    public record PositionTransportResponse(
        Guid TransportId,
        double X,
        double Y);
}
