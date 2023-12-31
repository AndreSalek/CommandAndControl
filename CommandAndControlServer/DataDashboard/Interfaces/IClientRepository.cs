using DataDashboard.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataDashboard.Interfaces
{
    public interface IClientRepository
    {
        Task<EntityEntry<Client>> AddClientAsync(Client client);
        Task AddClientHwInfoAsync(ClientHwInfo hwInfo);
        Task AddClientSessionDataAsync(SessionData info);
        Task DeleteClientAsync(Client client);
        Task DeleteClientHwInfoAsync(ClientHwInfo hwInfo);
        Task DeleteClientSessionDataAsync(SessionData data);
        Task<Client> GetClientAsync(string id);
        Task<ClientHwInfo> GetClientHwInfoByMACAsync(string mac);
        Task<IList<SessionData>> GetClientSessionDataAsync(int id);
        Task UpdateClientAsync(Client client);
        Task UpdateClientHwInfoAsync(ClientHwInfo hwInfo);
        Task UpdateClientSessionDataAsync(SessionData data);
    }
}