using CartAPI.DTOs;

namespace CartAPI.Services;

/// <summary>Giao tiếp với ProductAPI để lấy thông tin và kiểm tra tồn kho sản phẩm</summary>
public class ProductApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiService> _logger;

    public ProductApiService(IHttpClientFactory httpClientFactory, ILogger<ProductApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ProductAPI");
        _logger = logger;
    }

    /// <summary>Lấy thông tin một sản phẩm theo ID (bao gồm stock)</summary>
    public async Task<ProductInfoDto?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProductInfoDto>($"api/Products/{productId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("ProductAPI unavailable for product {Id}: {Msg}", productId, ex.Message);
            return null;
        }
    }

    /// <summary>Lấy thông tin nhiều sản phẩm song song</summary>
    public async Task<Dictionary<Guid, ProductInfoDto>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
    {
        var tasks = productIds.Select(async id =>
        {
            var product = await GetProductByIdAsync(id);
            return (id, product);
        });

        var results = await Task.WhenAll(tasks);

        return results
            .Where(r => r.product is not null)
            .ToDictionary(r => r.id, r => r.product!);
    }

    /// <summary>
    /// Kiểm tra xem số lượng yêu cầu có đủ tồn kho không.
    /// Trả về (isValid, stockQuantity, errorMessage)
    /// </summary>
    public async Task<(bool IsValid, int Stock, string? Error)> ValidateStockAsync(
        Guid productId, int requestedQty)
    {
        var product = await GetProductByIdAsync(productId);

        if (product is null)
            return (false, 0, $"Product {productId} not found or ProductAPI unavailable");

        if (product.StockQuantity <= 0)
            return (false, 0, $"Product '{product.ProductName}' is out of stock");

        if (requestedQty > product.StockQuantity)
            return (false, product.StockQuantity,
                $"Only {product.StockQuantity} item(s) available for '{product.ProductName}'");

        return (true, product.StockQuantity, null);
    }
}