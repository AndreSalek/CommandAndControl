using DataDashboard.Interfaces;
using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataDashboard.Data
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;
        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Client> GetClientAsync(string id) =>
            await _context.Clients.FindAsync(id);

        /// <summary>
        /// Retrieves a HwInfo from the database by MAC address
        /// </summary>
        /// <param name="mac"></param>
        /// <returns>Instance of ClientHwInfo that matches mac address parameter, else null</returns>
        public async Task<ClientHwInfo> GetClientHwInfoByMACAsync(string mac) =>
            await _context.HwInfo.Where(column => column.MAC == mac).SingleOrDefaultAsync();

        public async Task<IList<SessionData>> GetClientSessionDataAsync(int id) =>
            await _context.Sessions.Where(s => s.ClientId == id).ToListAsync();

        /// <summary>
        /// Adds a client to the database
        /// </summary>
        /// <param name="client"></param>
        /// <returns>EntityEntry of record written to database</returns>
        public async Task<EntityEntry<Client>> AddClientAsync(Client client)
        {
            EntityEntry<Client> record = await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task AddClientHwInfoAsync(ClientHwInfo hwInfo)
        {
            await _context.HwInfo.AddAsync(hwInfo);
            await _context.SaveChangesAsync();
        }
        public async Task AddClientSessionDataAsync(SessionData info)
        {
            await _context.Sessions.AddAsync(info);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClientAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClientHwInfoAsync(ClientHwInfo hwInfo)
        {
            _context.HwInfo.Update(hwInfo);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateClientSessionDataAsync(SessionData data)
        {
            _context.Sessions.Update(data);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClientAsync(Client client)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClientHwInfoAsync(ClientHwInfo hwInfo)
        {
            _context.HwInfo.Remove(hwInfo);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteClientSessionDataAsync(SessionData data)
        {
            _context.Sessions.Remove(data);
            await _context.SaveChangesAsync();
        }
    }
}
