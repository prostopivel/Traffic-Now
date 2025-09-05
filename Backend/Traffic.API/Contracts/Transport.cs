namespace Traffic.API.Contracts
{
    public record TransportResponse(
        Guid Id,
        Guid UserId,
        Guid PointId,
        string Url,
        PointResponse Point,
        bool IsActive);
}
