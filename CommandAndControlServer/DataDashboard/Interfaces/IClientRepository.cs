using DataDashboard.Models;

namespace DataDashboard.Interfaces
{
    public interface IClientRepository
    {
        Task<Client> GetClientAsync(ClientHwInfo clientInfo);
        Task<ScriptResult> GetScriptResultAsync(int id);
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<IEnumerable<ScriptResult>> GetAllScriptResultsAsync();
        Task<Client> CreateClientAsync(ClientHwInfo clientInfo);
        Task SaveScriptResultAsync(ScriptResult scriptResult);

        Task UpdateClientAsync(Client client);
        Task DeleteClientAsybc(int id);
        Task DeleteScriptResultAsync(int id);

        // CHECK
		Task<bool> IsNewClientAsync(ClientHwInfo clientInfo);
	}
}
