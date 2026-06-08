namespace PetCenterAPI.Service
{
    using Microsoft.EntityFrameworkCore;
    using PetCenterAPI.Models;
    using PetCenterAPI.Service.Interface;

    public class CleanupProductImageJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CleanupProductImageJob(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine(
                        $"Cleanup job running: {DateTime.Now}");

                    await ProcessCleanup(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Cleanup job error: {ex.Message}");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);
            }
        }

        private async Task ProcessCleanup(
            CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<PetCenterContext>();

            var cloudinaryService = scope.ServiceProvider
                .GetRequiredService<ICloudinaryService>();

            var threshold = DateTime.UtcNow.AddDays(-3);

            // 1️⃣ lấy ảnh inactive quá 3 ngày
            var images = await db.ProductImages
                .Where(x => !x.IsActive == true)
                .Where(x =>
                    x.InactiveAt != null &&
                    x.InactiveAt < threshold)
                .OrderBy(x => x.InactiveAt)
                .Take(100)
                .ToListAsync(token);

            if (!images.Any())
                return;

            var imageUrls = images
                .Select(x => x.ImageUrl)
                .ToList();

            // 2️⃣ lấy tất cả snapshot image đang dùng
            var orderUsedImages = await db.OrderProductSnapshots
                .Where(x =>
                    imageUrls.Contains(
                        x.ProductImage))
                .Select(x => x.ProductImage)
                .Distinct()
                .ToListAsync(token);

            var importUsedImages = await db.ImportProductSnapshots
                .Where(x =>
                    imageUrls.Contains(
                        x.ProductImage))
                .Select(x => x.ProductImage)
                .Distinct()
                .ToListAsync(token);

            var usedImages = orderUsedImages
                .Union(importUsedImages)
                .ToHashSet();

            // 3️⃣ cleanup
            foreach (var img in images)
            {
                try
                {
                    // ảnh đang được dùng
                    // trong historical transaction
                    if (usedImages.Contains(img.ImageUrl))
                    {
                        continue;
                    }

                    // xóa cloudinary
                    if (!string.IsNullOrEmpty(img.PublicId))
                    {
                        await cloudinaryService
                            .DeleteImageAsync(img.PublicId);
                    }

                    // xóa DB
                    db.ProductImages.Remove(img);

                    Console.WriteLine(
                        $"Deleted image: {img.ImageUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Delete image failed: {ex.Message}");
                }
            }

            await db.SaveChangesAsync(token);
        }
    }
}