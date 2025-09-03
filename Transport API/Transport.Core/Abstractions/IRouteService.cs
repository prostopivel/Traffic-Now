using Transport.Core.Models;

namespace Transport.Application.Services
{
    public interface IRouteService
    {
        int Speed { get; set; }
        void GenerateRoute();
        List<Point> GetCurrentRoute();
        Point? GetPoint();
        Point? PeekPoint();
        bool HasNextPoint();
        void ClearRoute();
    }
}