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

                //await Task.Delay(
                //    TimeSpan.FromSeconds(30),
                //    stoppingToken);
                await Task.Delay(
                    TimeSpan.FromDays(7),
                        stoppingToken);
            }
        }
        #region cleanup old version -Quoc
        //private async Task ProcessCleanup(CancellationToken token)
        //{
        //    using var scope = _scopeFactory.CreateScope();

        //    var db = scope.ServiceProvider.GetRequiredService<PetCenterContext>();
        //    var cloudinaryService = scope.ServiceProvider.GetRequiredService<ICloudinaryService>();

        //    var threshold = DateTime.Now.AddDays(-3);

        //    // 1️⃣ lấy ảnh inactive quá 3 ngày
        //    var images = await db.ProductImages
        //        .Where(x => !x.IsActive == true)
        //        .Where(x => x.InactiveAt != null && x.InactiveAt < threshold)
        //        .OrderBy(x => x.InactiveAt)
        //        .Take(100)
        //        .ToListAsync(token);

        //    if (!images.Any())
        //        return;

        //    var imageUrls = images
        //        .Select(x => x.ImageUrl)
        //        .ToList();

        //    // 2️⃣ lấy tất cả snapshot image đang dùng
        //    var orderUsedImages = await db.OrderProductSnapshots
        //        .Where(x => imageUrls.Contains(x.ProductImage))
        //        .Select(x => x.ProductImage)
        //        .Distinct()
        //        .ToListAsync(token);

        //    var importUsedImages = await db.ImportProductSnapshots
        //        .Where(x => imageUrls.Contains(x.ProductImage))
        //        .Select(x => x.ProductImage)
        //        .Distinct()
        //        .ToListAsync(token);

        //    var usedImages = orderUsedImages
        //        .Union(importUsedImages)
        //        .ToHashSet();

        //    // 3️⃣ cleanup
        //    foreach (var img in images)
        //    {
        //        try
        //        {
        //            // ảnh đang được dùng trong historical transaction
        //            if (usedImages.Contains(img.ImageUrl))
        //                continue;

        //            // xóa cloudinary
        //            if (!string.IsNullOrEmpty(img.PublicId))
        //            {
        //                await cloudinaryService.DeleteImageAsync(img.PublicId);
        //            }
        //            // xóa DB
        //            db.ProductImages.Remove(img);
        //            Console.WriteLine($"Deleted image: {img.ImageUrl}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Delete image failed: {ex.Message}");
        //        }
        //    }

        //    await db.SaveChangesAsync(token);
        //}
        #endregion
        private async Task ProcessCleanup(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PetCenterContext>();
            var cloudinaryService = scope.ServiceProvider.GetRequiredService<ICloudinaryService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CleanupProductImageJob>>();

            var threshold = DateTime.Now.AddDays(-3);

            // 1. Lấy tối đa 30 ảnh inactive
            var images = await db.ProductImages
                .Where(x => x.IsActive == false && x.InactiveAt != null && x.InactiveAt < threshold)
                .OrderBy(x => x.InactiveAt)
                .Take(30)
                .ToListAsync(token);

            if (!images.Any()) return;

            var imageUrls = images.Select(x => x.ImageUrl).Where(u => !string.IsNullOrEmpty(u)).Distinct().ToList();
            if (!imageUrls.Any()) return;

            var orderUsed = await db.OrderProductSnapshots.AsNoTracking()
                .Where(x => imageUrls.Contains(x.ProductImage)).Select(x => x.ProductImage).ToListAsync(token);
            var importUsed = await db.ImportProductSnapshots.AsNoTracking()
                .Where(x => imageUrls.Contains(x.ProductImage)).Select(x => x.ProductImage).ToListAsync(token);

            var usedImages = new HashSet<string>(orderUsed.Concat(importUsed));

            // 3. Xóa ảnh
            bool hasChanges = false;
            foreach (var img in images)
            {
                try
                {
                    if (!string.IsNullOrEmpty(img.ImageUrl) && usedImages.Contains(img.ImageUrl)) continue;

                    if (!string.IsNullOrEmpty(img.PublicId))
                        await cloudinaryService.DeleteImageAsync(img.PublicId);

                    db.ProductImages.Remove(img);
                    hasChanges = true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Lỗi xóa ảnh ID: {Id}", img.ImageId);
                }
            }
            if (hasChanges)
                await db.SaveChangesAsync(token);
        }
    }
}