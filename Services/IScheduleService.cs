
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
        Task<bool> CancelTripByDriverAsync(int tripId, string reason);

        Task<List<ScheduleDto>> GetPendingSchedulesByRunAsync(string runLogin, DateTime date);
        Task<List<ScheduleDto>> GetFutureSchedulesByRunAsync(string runLogin);
        Task<List<ScheduleHistoryDto>> GetScheduleHistoryAsync(string runLogin, DateTime date);
        Task<int> GetScheduleHistoryCountAsync(string runLogin, DateTime date);

    }
}
