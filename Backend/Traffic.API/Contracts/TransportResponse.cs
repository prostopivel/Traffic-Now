namespace Traffic.API.Contracts
{
    public record TransportResponse(
        Guid Id,
        Guid UserId,
        Guid PointId,
        PointResponse Point,
        double X,
        double Y);
}
