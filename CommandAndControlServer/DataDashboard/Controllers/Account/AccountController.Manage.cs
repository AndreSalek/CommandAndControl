﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataDashboard.Controllers.Account
{
    [Authorize]
    public partial class AccountController : Controller
    {
        [HttpGet("{controller}/Manage/Index")]
        public IActionResult ProfileSettings()
        {
            return View("Manage/Index");
        }
    }
}
