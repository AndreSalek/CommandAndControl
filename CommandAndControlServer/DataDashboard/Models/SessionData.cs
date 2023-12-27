using DataDashboard.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace DataDashboard.Models
{
    [EntityTypeConfiguration(typeof(SessionDataConfiguration))]
    public class SessionData
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string SessionId { get; set; } // From HttpContext.Session.Id
    }
}
