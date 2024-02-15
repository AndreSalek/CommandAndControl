using DataDashboard.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace DataDashboard.Models
{
    [EntityTypeConfiguration(typeof(ClientHwInfoConfiguration))]
    public class ClientHwInfo
    {
        public int Id { get; set; }
        public string MAC { get; set; } = default!;
        public string? OS { get; set; }
        public string? CpuId { get; set; }
        public int? RAMCapacity { get; set; } 
    }
}
