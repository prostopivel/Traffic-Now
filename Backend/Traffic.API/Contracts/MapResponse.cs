namespace Traffic.API.Contracts
{
    public record MapResponse(
        Guid Id,
        string Name,
        List<PointResponse> Points);
}
