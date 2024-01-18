using DataDashboard.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace DataDashboard.Models
{
    [EntityTypeConfiguration(typeof(ConnectionDataConfiguration))]
    public class ConnectionData
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ConnectionId { get; set; } = default!;
        public string IP { get; set; } = default!;
        public DateTime ConnectedAt { get; set; }
    }
}
