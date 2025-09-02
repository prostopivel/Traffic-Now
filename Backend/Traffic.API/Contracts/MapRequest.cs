namespace Traffic.API.Contracts
{
    public record MapRequest(
        Guid Id,
        string Name);

    public record MapRequestCreate(
        string Name);
}
