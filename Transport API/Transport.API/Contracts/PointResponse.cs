namespace Transport.API.Contracts
{
    public record PointResponse(
        Guid TransportId,
        Guid MapId,
        double X,
        double Y);
}