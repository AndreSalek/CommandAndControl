﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using DataDashboard.Models;
using DataDashboard.Data.Config;
using DataDashboard.ViewModels;

namespace DataDashboard.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<ConnectionData> Sessions { get; set; }
        public DbSet<ClientHwInfo> HwInfo { get; set; }
        public DbSet<Script> Scripts { get; set; }
        public DbSet<ScriptResult> ScriptResults { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    }
}
