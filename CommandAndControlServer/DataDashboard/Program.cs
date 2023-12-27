using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Azure.Core;
using DataDashboard.Data;
using DataDashboard.Controllers;
using DataDashboard.BLL;

namespace DataDashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Retrieve connectiong string from file
            string connectionString = builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("This database does not exist.");

            //Add Services for dependency injection into controllers
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)
            );
            // Users represent the accounts that are registered to the website and can access the dashboard
            builder.Services.AddScoped<DbContext>(services => services.GetRequiredService<ApplicationDbContext>());
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                            .AddUserStore<UserStore<IdentityUser>>()
                            .AddDefaultTokenProviders();
            builder.Services.AddAntiforgery();
            builder.Services.AddScoped<UserManager<IdentityUser>>();
            builder.Services.AddScoped<SignInManager<IdentityUser>>();
            builder.Services.AddScoped<ILogger, Logger<AccountController>>();
            builder.Services.AddScoped<ILogger, Logger<ClientController>>();
            builder.Services.AddScoped<ILogger, Logger<ConnectionPipeline>>();
            // Clients represent the 'endpoints' that are connected to the server through websocket
            builder.Services.AddScoped<ClientRepository>();
            builder.Services.AddScoped<ConnectionPipeline>();
            //TODO: EmailSender service implementation

            //Add services to the container.
            //Add global AntiForgeryToken filter
            builder.Services.AddControllersWithViews(options =>
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())
                );

            var app = builder.Build();

            //Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {

                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseWebSockets();

            app.UseRouting();

            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "Account",
                pattern: "{controller=Account}/{action=Login}");
            app.MapControllerRoute(
                name: "Bot",
                pattern: "{controller=Bot}/{action=Index}");

            app.Run();
        }
    }
}