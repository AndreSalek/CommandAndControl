using DataDashboard.Data.Config;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;

namespace DataDashboard.Models
{
    [EntityTypeConfiguration(typeof(ClientConfiguration))]
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ClientHwInfo clientHwInfo { get; set; }
        public IList<SessionData> SessionsHistory { get; set; }
    }
}
