using Interchée.Data;
using Interchée.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Interchée.Services
{
    public class AssignmentAutoCloseService : BackgroundService
    {
        private readonly ILogger<AssignmentAutoCloseService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes

        public AssignmentAutoCloseService(
            ILogger<AssignmentAutoCloseService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Assignment Auto-Close Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var statusService = scope.ServiceProvider.GetRequiredService<AssignmentStatusService>();

                        await statusService.AutoUpdateExpiredAssignments();

                        _logger.LogInformation("Checked and updated expired assignments.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while auto-closing assignments.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}