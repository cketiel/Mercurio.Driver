using Mercurio.Driver.DTOs; 
using System.Threading.Tasks;

namespace Mercurio.Driver.Services
{
    public interface IProviderService
    {
        Task<ProviderDto?> GetContactProviderAsync(); 
        Task<bool> UpdateContactProviderAsync(ProviderDto provider);
    }
}