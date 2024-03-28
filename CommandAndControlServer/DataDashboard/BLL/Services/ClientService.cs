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
        private FileSystemWatcher watcher = new FileSystemWatcher("Scripts");
        private IServiceProvider _provider;

        public ConcurrentBag<int> ConnectedClients { get; } = new ConcurrentBag<int>();
		public CancellationToken CancellationToken { get => _cancellationTokenSource.Token; }
        public TaskCompletionSource<Script> ScriptToExecute { get; } = new TaskCompletionSource<Script>();

		public ClientService(ILogger<IClientService> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
            SetupWatcher();
            CheckDifferences();
        }

        private void CheckDifferences()
        {
            using var scope = _provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            //Path.GetFileNameWithoutExtension(Directory.GetFiles(watcher.Path));
            var savedScripts = context.Scripts.ToList();
            SortedList<string, string> scriptNamesLocal = new SortedList<string, string>();
            foreach (var file in Directory.GetFiles(watcher.Path))
            {
                var f = Path.GetFileName(file).Split(".");
                scriptNamesLocal.Add(f.First(), GeneralUtil.GetShellNameFromExtension(f.Last()));
            }

            for (int i = 0; i < scriptNamesLocal.Count; i++)
            {
                bool existsInDb = savedScripts.Any(s => s.Name == scriptNamesLocal.Keys[i]);
                if (existsInDb) { }
                else
                {
                    var script = new Script()
                    {
                        Name = scriptNamesLocal.Keys[i],
                        Shell = Enum.Parse<ShellType>(scriptNamesLocal.Values[i])
                    };
                    context.Scripts.Add(script);
                    context.SaveChanges();
                }
            }
        }

        private void SetupWatcher()
        {
            watcher.NotifyFilter = NotifyFilters.FileName
               | NotifyFilters.LastWrite;
            watcher.Filters.Add("*.ps1");
            watcher.Filters.Add("*.cmd");
            watcher.Filters.Add("*.bat");
            watcher.Filters.Add("*.sh");
            watcher.Filters.Add("*.py");
            watcher.Created += new FileSystemEventHandler(OnCreatedOrDeleted);
            watcher.Deleted += new FileSystemEventHandler(OnCreatedOrDeleted);
            watcher.Changed += new FileSystemEventHandler(OnCreatedOrDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            string fileName = Path.GetFileName(e.FullPath);
            string[] file = fileName.Split(".", StringSplitOptions.TrimEntries);
            string shell = GeneralUtil.GetShellNameFromExtension(file.Last());

            using var scope = _provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            var script = context.Scripts.FirstOrDefault(s => s.Name == e.OldName);
            if (script == null)
            {
                context.Scripts.Add(new Script
                {
                    Name = fileName,
                    Shell = Enum.Parse<ShellType>(shell)
                });
                return;
            }
            script.Name = e.Name;
            context.Scripts.Update(script);
            context.SaveChanges();
        }

        protected  void OnCreatedOrDeleted(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            //Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            string fileName = Path.GetFileName(e.FullPath);
            string[] file = fileName.Split(".", StringSplitOptions.TrimEntries);
            string shell = GeneralUtil.GetShellNameFromExtension(file.Last());

            using var scope = _provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            var script = new Script()
            {
                Name = fileName,
                Shell = Enum.Parse<ShellType>(shell)
            };

            if (e.ChangeType == WatcherChangeTypes.Created) context.Scripts.Add(script);
            else
            {
                context.Scripts.Remove(context.Scripts.Where(item => item.Name == fileName).First());
                
            }
            context.SaveChanges();
        }

        public void Dispose()
        {
            watcher.Renamed -= OnRenamed;
            watcher.Created -= OnCreatedOrDeleted;
            watcher.Deleted -= OnCreatedOrDeleted;
            this.watcher.Dispose();
        }
    }
}
