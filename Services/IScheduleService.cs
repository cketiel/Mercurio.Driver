
using Mercurio.Driver.DTOs;

namespace Mercurio.Driver.Services
{
    public interface IScheduleService
    {
        Task<List<ScheduleDto>> GetSchedulesByRunAsync(string runLogin, DateTime date);
        Task<List<ScheduleDto>> GetTodayScheduleAsync();
    }
}
