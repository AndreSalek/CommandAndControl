using DataDashboard.Models;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using DataDashboard.Interfaces;

namespace DataDashboard.Data
{
	public class ClientRepository : IClientRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<ClientRepository> _logger;

		public ClientRepository(ApplicationDbContext context, ILogger<ClientRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// Checks if client is new by comparing MAC address
		/// </summary>
		public async Task<bool> IsNewClientAsync(ClientHwInfo hwInfo)
		{
			return await _context.HwInfo.SingleOrDefaultAsync(dbInfo => dbInfo.MAC == hwInfo.MAC) == null ? true : false;
		}
		/// <summary>
		/// Creates new client with ClientHwInfo and saves it to database
		/// </summary>
		public async Task<Client> CreateClientAsync(ClientHwInfo clientInfo)
		{
			// Create client and write it to database, Id is generated there
			var client = new Client();
			EntityEntry<Client> record = await _context.AddAsync(client);

			// Retrieve client Id from the entry and add it as primary key for ClientHwInfo
			//clientInfo.Id = record.Property(prop => prop.Id).CurrentValue;
			await _context.HwInfo.AddAsync(clientInfo);
			var dbSave = _context.SaveChangesAsync();

			Client result = record.Entity;
			//result.clientHwInfo = clientInfo;
			await dbSave;
			return result;
		}

		/// <summary>
		/// Retrieves Client from database
		/// </summary>
		public async Task<Client> GetClientAsync(ClientHwInfo clientInfo) => await _context.Clients.SingleAsync(db => db.Id == clientInfo.Id);

		public Task<ScriptResult> GetScriptResultAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Client>> GetAllClientsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<ScriptResult>> GetAllScriptResultsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<Client> SaveClientAsync(ClientHwInfo clientInfo)
		{
			throw new NotImplementedException();
		}

		public Task SaveScriptResultAsync(ScriptResult scriptResult)
		{
			throw new NotImplementedException();
		}

		public Task UpdateClientAsync(Client client)
		{
			throw new NotImplementedException();
		}

		public Task DeleteClientAsybc(int id)
		{
			throw new NotImplementedException();
		}

		public Task DeleteScriptResultAsync(int id)
		{
			throw new NotImplementedException();
		}
	}
}
