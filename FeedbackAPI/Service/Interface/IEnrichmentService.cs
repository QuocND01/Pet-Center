namespace FeedbackAPI.Service.Interface
{
    public interface IEnrichmentService
    {
        Task<(string? FullName, string? Email)> GetCustomerInfoAsync(Guid customerId);
        Task<(string? ProductName, string? ImageUrl)> GetProductInfoAsync(Guid productId);
        Task<string?> GetStaffNameAsync(Guid staffId);
    }
}
