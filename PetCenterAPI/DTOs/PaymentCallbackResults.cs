namespace PetCenterAPI.DTOs
{
    public class VnPayCallbackResult
    {
        public string TransactionRef { get; set; } = string.Empty;
        public string GatewayTransactionNo { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BankCode { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string RawData { get; set; } = string.Empty;
    }

    public class MoMoCallbackResult
    {
        public string TransactionRef { get; set; } = string.Empty;
        public string GatewayTransactionNo { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsSuccess { get; set; }
        public string RawData { get; set; } = string.Empty;
    }
}