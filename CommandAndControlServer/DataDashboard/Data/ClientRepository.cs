using DataDashboard.Interfaces;
using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace DataDashboard.Data
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;
        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Client> GetClient(string id) =>
            await _context.Clients.FindAsync(id);
        public async Task<ClientHwInfo> GetClientHwInfo(int clientId) =>
            await _context.HwInfo.FindAsync(clientId);

        public async Task<IList<SessionData>> GetClientSessionData(int id) =>
            await _context.Sessions.Where(s => s.ClientId == id).ToListAsync();

        public async Task AddClient(Client client)
        {
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
        }

        public async Task AddClientHwInfo(ClientHwInfo hwInfo)
        {
            await _context.HwInfo.AddAsync(hwInfo);
            await _context.SaveChangesAsync();
        }
        public async Task AddClientSessionData(SessionData info)
        {
            await _context.Sessions.AddAsync(info);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClient(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClientHwInfo(ClientHwInfo hwInfo)
        {
            _context.HwInfo.Update(hwInfo);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateClientSessionData(SessionData data)
        {
            _context.Sessions.Update(data);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClient(Client client)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClientHwInfo(ClientHwInfo hwInfo)
        {
            _context.HwInfo.Remove(hwInfo);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteClientSessionData(SessionData data)
        {
            _context.Sessions.Remove(data);
            await _context.SaveChangesAsync();
        }
    }
}
