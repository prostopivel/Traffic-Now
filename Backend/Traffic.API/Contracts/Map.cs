namespace Traffic.API.Contracts
{
    public record MapResponse(
        Guid Id,
        string Name,
        List<PointResponse> Points);

    public record MapRequest(
        Guid Id,
        string Name);

    public record MapRequestCreate(
        string Name);
}
