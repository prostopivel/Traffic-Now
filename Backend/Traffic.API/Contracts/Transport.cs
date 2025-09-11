namespace Traffic.API.Contracts
{
    public record TransportResponse(
        Guid Id,
        Guid PointId,
        string Url,
        PointResponse Point,
        bool IsActive);

    public record FirstTransportResponse(
        double X,
        double Y,
        int Speed);
}
