namespace Traffic.API.Contracts
{
    public record PointResponse(
        Guid Id,
        Guid MapId,
        double X,
        double Y,
        string Name,
        List<Guid> ConnectedPointsIds);
}
