using DataDashboard.Models;

namespace DataDashboard.Interfaces
{
    public interface IClientRepository
    {
        Task AddClient(Client client);
        Task AddClientHwInfo(ClientHwInfo hwInfo);
        Task AddClientSessionData(SessionData info);
        Task DeleteClient(Client client);
        Task DeleteClientHwInfo(ClientHwInfo hwInfo);
        Task DeleteClientSessionData(SessionData data);
        Task<Client> GetClient(string id);
        Task<ClientHwInfo> GetClientHwInfo(int clientId);
        Task<IList<SessionData>> GetClientSessionData(int id);
        Task UpdateClient(Client client);
        Task UpdateClientHwInfo(ClientHwInfo hwInfo);
        Task UpdateClientSessionData(SessionData data);
    }
}