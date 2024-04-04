using DataDashboard.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace DataDashboard.Models
{
    [EntityTypeConfiguration(typeof(ScriptResultConfig))]
    public class ScriptResult
    {
        public int Id { get; set; }
        public int CommandId { get; set; }
        public int ClientId { get; set; }
        public string Content { get; set; }
        public bool IsError { get; set; }
    }
}
