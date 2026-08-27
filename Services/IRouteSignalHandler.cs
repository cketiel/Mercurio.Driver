using Raphael.Driver.DTOs;

namespace Raphael.Driver.Services
{
    /// <summary>
    /// A screen that can react to the route changing underneath it.
    /// </summary>
    /// <remarks>
    /// Screens register while they are on top and unregister when they leave, so a signal is
    /// only ever offered to what the driver is actually looking at.
    /// </remarks>
    public interface IRouteSignalHandler
    {
        Task<RouteSignalOutcome> HandleRouteSignalAsync(NotificationDto signal);
    }
}
