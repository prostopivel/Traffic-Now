using Transport.Core.Models;

namespace Transport.Core.Abstractions
{
    public interface IRouteRepository
    {
        int Speed { get; set; }
        void GenerateRoute(Map map, Guid garageId);
        List<Point> GetCurrentRoute();
        Point? GetPoint();
        Point? PeekPoint();
        bool HasNextPoint();
        void ClearRoute();
    }
}