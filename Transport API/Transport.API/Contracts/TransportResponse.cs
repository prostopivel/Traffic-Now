namespace Transport.API.Contracts
{
    public record TransportResponse(
        Guid Id,
        Guid mapId,
        Guid pointId);
}
