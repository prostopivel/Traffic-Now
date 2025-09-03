using Traffic.Core.Models;

namespace Traffic.API.Contracts
{
    public class ContractsFactory
    {
        public static PointResponse CreatePointResponse(Point point)
        {
            var pointResponse = new PointResponse(
                point.Id,
                point.MapId,
                point.X,
                point.Y,
                point.Name,
                point.ConnectedPointsIds);

            return pointResponse;
        }

        public static (Map?, string Error) CreateMap(MapRequest mapRequest)
        {
            (var map, var Error) = Map.Create(
                Guid.NewGuid(),
                mapRequest.Name);

            return (map, Error);
        }

        public static MapResponse CreateMapResponse(Map map)
        {
            var pointResponses = new List<PointResponse>(map.Points.Count);
            foreach (var point in map?.Points ?? [])
            {
                pointResponses.Add(CreatePointResponse(point));
            }

            var mapResponse = new MapResponse(
                map!.Id,
                map.Name,
                pointResponses);

            return mapResponse;
        }

        public static (Transport?, string Error) CreateTransport(Transport transportRequest)
        {
            return (null, string.Empty);
        }

        public static TransportResponse CreateTransportResponse(Transport transport)
        {
            var pointResponse = CreatePointResponse(transport.Point);

            var transportResponse = new TransportResponse(
                transport.Id,
                transport.UserId,
                transport.PointId,
                pointResponse,
                transport.X,
                transport.Y);

            return transportResponse;
        }

        public static (Core.Models.Route?, string Error) CreateRoute(RouteRequest routeRequest)
        {
            (var route, var Error) = Core.Models.Route.Create(
                Guid.NewGuid(),
                routeRequest.TransportId,
                routeRequest.RouteTime);

            return (route, Error);
        }

        public static RouteResponse CreateRouteResponse(Core.Models.Route route)
        {
            var transportResponse = CreateTransportResponse(route.Transport);

            var pointResponses = new List<PointResponse>(route.Points.Count);
            foreach (var point in route.Points)
            {
                pointResponses.Add(CreatePointResponse(point));
            }

            var routeResponse = new RouteResponse(
                route.Id,
                route.TransportId,
                transportResponse,
                route.RouteTime,
                pointResponses);

            return routeResponse;
        }
    }
}
