namespace DataDashboard.Models
{
    public class ScriptResult
    {
        public int CommandId { get; set; }
        public int ClientId { get; set; }
        public string Content { get; set; }
        public bool IsError { get; set; }
    }
}
