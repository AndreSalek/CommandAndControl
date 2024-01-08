using DataDashboard.BLL;

namespace DataDashboard.Models
{
    public class Script
    {
        public int Id { get; set; }
        public string[] Lines { get; set; } = null!;
        public ShellType Shell { get; set; }
    }
}
