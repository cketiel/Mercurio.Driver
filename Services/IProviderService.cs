using Raphael.Driver.DTOs; 
using System.Threading.Tasks;

namespace Raphael.Driver.Services
{
    public interface IProviderService
    {
        Task<ProviderDto?> GetContactProviderAsync(); 
        Task<bool> UpdateContactProviderAsync(ProviderDto provider);
    }
}