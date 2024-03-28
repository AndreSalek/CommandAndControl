using DataDashboard.BLL;
using Microsoft.IdentityModel.Tokens;

namespace DataDashboard.Utility
{
    public static class GeneralUtil
    {
        public static string GetShellNameFromExtension(string extension)
        {
            if (extension.IsNullOrEmpty()) return "";
            extension = extension.ToLower().Trim();
            if (extension == "ps1") return "PowerShell";
            else if (extension == "cmd" || extension == "bat") return "CommandShell";
            else if (extension == "sh") return "Bash";            // Not necessarily true, but it is a convention
            else if (extension == "py") return "Python";
            else return "";
        }
    }
}
