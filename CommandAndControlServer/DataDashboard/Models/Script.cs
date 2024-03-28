using DataDashboard.BLL;
using DataDashboard.Data.Config;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataDashboard.Models
{
    [EntityTypeConfiguration(typeof(ScriptConfiguration))]
    public class Script
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string[]? Lines { get; set; }
        public ShellType Shell { get; set; }
    }
}
