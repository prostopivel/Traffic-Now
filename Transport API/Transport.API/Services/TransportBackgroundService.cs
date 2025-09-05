using Transport.Application.Services;
using Transport.Core.Abstractions;
using Transport.Core.Models;

namespace Transport.API.Services
{
    public class TransportHostService : BackgroundService
    {
        private const int MIN_WAIT_MINUTES = 2, MAX_WAIT_MINUTES = 3;
        private const double COORDINATE_SCALE = 100.0;
        private readonly IRouteService _routeService;
        private readonly IDataService _dataService;
        private readonly ILogger<TransportHostService> _logger;

        private DateTime _nextRouteTime;
        private Point? _currentTargetPoint = null;
        private Point? _previousPoint = null;
        private double _progressToTarget = 0;

        public TransportHostService(IRouteService routeService, IDataService dataService, ILogger<TransportHostService> logger)
        {
            _routeService = routeService;
            _dataService = dataService;
            _logger = logger;

            var random = new Random();
            _nextRouteTime = DateTime.Now.AddSeconds(
                random.Next(0, 120));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Transport Host Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ProcessMovement(stoppingToken);
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing transport movement");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            _logger.LogInformation("Transport Host Service stopped");
        }

        private void ProcessMovement(CancellationToken stoppingToken)
        {
            if (_nextRouteTime < DateTime.Now)
            {
                var random = new Random();
                _nextRouteTime = DateTime.Now.AddSeconds(
                    random.Next(MIN_WAIT_MINUTES * 60, MAX_WAIT_MINUTES * 60));

                if (!_routeService.HasNextPoint())
                {
                    _dataService.IsActive = true;
                    _routeService.GenerateRoute();
                    _logger.LogInformation("New route generated. Next route time: {NextRouteTime}", _nextRouteTime);
                }
            }

            if (_currentTargetPoint == null && _routeService.HasNextPoint())
            {
                _currentTargetPoint = _routeService.GetPoint();
                _previousPoint = _dataService.Transport.PointId != Guid.Empty
                    ? _dataService.Map.Points.FirstOrDefault(p => p.Id == _dataService.Transport.PointId)
                    : null;
                _progressToTarget = 0;

                if (_currentTargetPoint != null)
                {
                    //_logger.LogInformation("Moving to new target point: {PointId}", _currentTargetPoint.Id);
                }
            }

            if (_currentTargetPoint != null && _previousPoint != null)
            {
                MoveTowardsTarget();
            }
            else if (!_routeService.HasNextPoint() && _currentTargetPoint == null)
            {
                _dataService.IsActive = false;
                _logger.LogDebug("No active route. Transport is stationary.");
            }
        }

        private void MoveTowardsTarget()
        {
            if (_currentTargetPoint == null || _previousPoint == null) return;

            double scaledPreviousX = _previousPoint.X * COORDINATE_SCALE;
            double scaledPreviousY = _previousPoint.Y * COORDINATE_SCALE;
            double scaledTargetX = _currentTargetPoint.X * COORDINATE_SCALE;
            double scaledTargetY = _currentTargetPoint.Y * COORDINATE_SCALE;

            double distanceX = scaledTargetX - scaledPreviousX;
            double distanceY = scaledTargetY - scaledPreviousY;
            double totalDistanceMeters = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            if (totalDistanceMeters <= 10)
            {
                _currentTargetPoint = null;
                return;
            }

            double speedMetersPerSecond = _routeService.Speed * 1000 / 3600;

            double distancePerSecond = speedMetersPerSecond;

            double progressIncrement = distancePerSecond / totalDistanceMeters;

            _progressToTarget += progressIncrement;

            if (_progressToTarget >= 1.0)
            {
                _dataService.Transport.X = _currentTargetPoint.X;
                _dataService.Transport.Y = _currentTargetPoint.Y;

                _logger.LogDebug("Reached target point: {PointId}", _currentTargetPoint.Id);
                _currentTargetPoint = null;
                _progressToTarget = 0;
            }
            else
            {
                double currentX = _previousPoint.X + (_currentTargetPoint.X - _previousPoint.X) * _progressToTarget;
                double currentY = _previousPoint.Y + (_currentTargetPoint.Y - _previousPoint.Y) * _progressToTarget;

                _dataService.Transport.X = currentX;
                _dataService.Transport.Y = currentY;

                _logger.LogDebug("Moving to point {PointId}. Progress: {Progress:P}, Position: ({X}, {Y})",
                    _currentTargetPoint.Id, _progressToTarget, currentX, currentY);
            }
        }
    }
}