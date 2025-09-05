namespace Traffic.Core.Models
{
    public record TransportJsonResponse(
        Guid Id,
        Guid PointId,
        double X,
        double Y);
}
