
using Mercurio.Driver.DTOs;

namespace Mercurio.Driver.Services
{
    public interface IScheduleService
    {
        Task<List<ScheduleDto>> GetSchedulesByRunAsync(string runLogin, DateTime date);
        Task<List<ScheduleDto>> GetTodayScheduleAsync();
        Task<bool> UpdateScheduleAsync(ScheduleDto scheduleToUpdate);
        Task<bool> SaveSignatureAsync(int scheduleId, string signatureBase64);
        Task<byte[]?> GetSignatureAsync(int scheduleId);
    }
}
