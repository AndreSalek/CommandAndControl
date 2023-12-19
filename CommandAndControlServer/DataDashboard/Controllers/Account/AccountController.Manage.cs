using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataDashboard.Controllers.Account
{
    [Authorize]
    public partial class AccountController : Controller
    {
        [Route("{controller}/Manage/Index")]
        public async Task<IActionResult> ProfileSettings()
        {
            return View("Manage/Index");
        }
    }
}
