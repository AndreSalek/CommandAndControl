using DataDashboard.Data;
using DataDashboard.Helpers;
using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NuGet.Versioning;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Timers;
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

		public ConcurrentBag<int> ConnectedClients { get; } = new ConcurrentBag<int>();
		public CancellationToken CancellationToken { get => _cancellationTokenSource.Token; }
        public TaskCompletionSource<Script> ScriptToExecute { get; } = new TaskCompletionSource<Script>();

		public ClientService(ILogger<IClientService> logger)
        {
            _logger = logger;
        } 
    }
}
