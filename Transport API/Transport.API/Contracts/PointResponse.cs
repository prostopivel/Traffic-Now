namespace Transport.API.Contracts
{
    public record PointResponse(
        Guid TransportId,
        double X,
        double Y);
}