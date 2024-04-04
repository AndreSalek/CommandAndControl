using DataDashboard.Data;
using DataDashboard.Helpers;
using DataDashboard.Models;
using DataDashboard.Utility;
using DataDashboard.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NuGet.Versioning;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Timers;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace DataDashboard.BLL.Services
{
	/// <summary>
	/// Service for managing connected clients and and database operations related to clients
	/// </summary>
	public class ClientService : IClientService
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ILogger<IClientService> _logger;
        private IServiceProvider _provider;

        public ConcurrentBag<int> ConnectedClients { get; } = new ConcurrentBag<int>();
		public CancellationToken CancellationToken { get => _cancellationTokenSource.Token; }
        public TaskCompletionSource<Script> ScriptToExecute { get; set; } = new TaskCompletionSource<Script>();

		public ClientService(ILogger<IClientService> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public void CheckDifferences()
        {
            using var scope = _provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            // Files in Scripts directory
            var files = Directory.GetFiles("Scripts").ToList();
            // Files saved in DB (for view)
            var savedScripts = context.Scripts.ToList();

            for (int i = files.Count() - 1; i >= 0; i--)
            {
                var scriptDb = savedScripts.Where(script => script.Name == Path.GetFileNameWithoutExtension(files[i])).FirstOrDefault();
                var isInDatabase = scriptDb != null ? true : false;
                if (isInDatabase)
                {
                    savedScripts.Remove(scriptDb);
                }
                else
                {
                    string[] splitName = Path.GetFileName(files[i]).Split(".");

                    Script script = new Script()
                    {
                        Name = splitName[0],
                        Shell = Enum.Parse<ShellType>(GeneralUtil.GetShellNameFromExtension(splitName[1]))
                    };
                    context.Scripts.Add(script);
                    context.SaveChanges();
                }
            }

            if (savedScripts.Count > 0)
            {
                // Remove all scripts that were deleted in Scripts folder from database
                savedScripts.ForEach(script => context.Scripts.Remove(script));
            }
            else if (savedScripts.Count == 0) return;
            

           
        }
    }
}
