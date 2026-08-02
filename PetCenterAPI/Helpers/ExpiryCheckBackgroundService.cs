using Cronos;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.BackgroundServices
{
    public class ExpiryCheckBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiryCheckBackgroundService> _logger;
        private readonly CronExpression _cronExpression = CronExpression.Parse("5 0 * * *"); // 00:05 mỗi ngày
        private readonly TimeZoneInfo _vietnamTimeZone;

        public ExpiryCheckBackgroundService(IServiceProvider serviceProvider, ILogger<ExpiryCheckBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            try
            {
                _vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                _vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(">>> EXPIRY CHECK BACKGROUND SERVICE STARTED <<<");

            // 1. Chạy quét 1 lần ngay khi Web API vừa bật lên
            await ExecuteAllExpiryChecksAsync(stoppingToken);

            // 2. Vòng lặp chờ đến 00:05 sáng mỗi ngày
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowUtc = DateTime.Now;
                    var nextRunUtc = _cronExpression.GetNextOccurrence(nowUtc, _vietnamTimeZone);

                    if (nextRunUtc.HasValue)
                    {
                        var delay = nextRunUtc.Value - nowUtc;
                        if (delay > TimeSpan.Zero)
                        {
                            await Task.Delay(delay, stoppingToken);
                        }

                        if (!stoppingToken.IsCancellationRequested)
                        {
                            await ExecuteAllExpiryChecksAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Lỗi trong vòng lặp Cron Background Task!");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task ExecuteAllExpiryChecksAsync(CancellationToken stoppingToken)
        {
            // Tạo Scope ngắn hạn để Resolve các Scoped Service & Repository
            using (var scope = _serviceProvider.CreateScope())
            {
                var inventoryExpiryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
                var appointmentExpiryService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();

                try
                {
                    // Chạy song song 2 tác vụ để tối ưu
                    await Task.WhenAll(
                        inventoryExpiryService.ProcessExpiredBatchesAsync(stoppingToken),
                        appointmentExpiryService.ProcessExpiredAppointmentsAsync(stoppingToken)
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Lỗi khi thực thi quét hết hạn tự động!");
                }
            }
        }
    }
}