using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetCenterAPI.DTOs;
using PetCenterAPI.Service.Interface;
using System.Net.Http.Json;

namespace PetCenterAPI.Service
{
    public class MoMoService : IMoMoService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MoMoService> _logger;

        public MoMoService(IConfiguration configuration, HttpClient httpClient, ILogger<MoMoService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> CreatePaymentUrlAsync(Guid orderId, decimal amount, string transactionRef, string orderInfo)
        {
            var partnerCode = _configuration["MoMo:PartnerCode"];
            var accessKey = _configuration["MoMo:AccessKey"];
            var secretKey = _configuration["MoMo:SecretKey"];
            var endpoint = _configuration["MoMo:Endpoint"];
            var returnUrl = _configuration["MoMo:ReturnUrl"];
            var ipnUrl = _configuration["MoMo:IpnUrl"];
            var requestType = _configuration["MoMo:RequestType"];

            var rawSignature = $"accessKey={accessKey}&amount={(long)amount}&extraData=&ipnUrl={ipnUrl}&orderId={transactionRef}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={transactionRef}&requestType={requestType}";
            var signature = HmacSha256(secretKey ?? "", rawSignature);

            var requestData = new
            {
                partnerCode = partnerCode,
                partnerName = "Test",
                storeId = "MomoTestStore",
                requestId = transactionRef,
                amount = (long)amount,
                orderId = transactionRef,
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = ipnUrl,
                lang = "vi",
                extraData = "",
                requestType = requestType,
                signature = signature
            };

            var response = await _httpClient.PostAsJsonAsync(endpoint, requestData);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create MoMo payment URL. Status: {Status}", response.StatusCode);
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            if (responseObj.TryGetProperty("resultCode", out var resultCode) && resultCode.GetInt32() == 0)
            {
                if (responseObj.TryGetProperty("payUrl", out var payUrl))
                {
                    return payUrl.GetString();
                }
            }

            _logger.LogError("MoMo API Error: {Response}", jsonResponse);
            return null;
        }

        public bool ValidateCallback(string rawBody, string signature)
        {
            var secretKey = _configuration["MoMo:SecretKey"];
            var json = JsonSerializer.Deserialize<JsonElement>(rawBody);
            
            var accessKey = json.GetProperty("accessKey").GetString();
            var amount = json.GetProperty("amount").GetInt64();
            var extraData = json.GetProperty("extraData").GetString();
            var message = json.GetProperty("message").GetString();
            var orderId = json.GetProperty("orderId").GetString();
            var orderInfo = json.GetProperty("orderInfo").GetString();
            var orderType = json.GetProperty("orderType").GetString();
            var partnerCode = json.GetProperty("partnerCode").GetString();
            var payType = json.GetProperty("payType").GetString();
            var requestId = json.GetProperty("requestId").GetString();
            var responseTime = json.GetProperty("responseTime").GetInt64();
            var resultCode = json.GetProperty("resultCode").GetInt32();
            var transId = json.GetProperty("transId").GetInt64();

            var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";

            var expectedSignature = HmacSha256(secretKey ?? "", rawHash);
            return expectedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }

        public MoMoCallbackResult ParseCallback(JsonElement body, string rawBody)
        {
            var resultCode = body.GetProperty("resultCode").GetInt32();
            var result = new MoMoCallbackResult
            {
                TransactionRef = body.GetProperty("orderId").GetString() ?? "",
                GatewayTransactionNo = body.GetProperty("transId").GetInt64().ToString(),
                ResponseCode = resultCode.ToString(),
                Amount = body.GetProperty("amount").GetInt64(),
                IsSuccess = resultCode == 0,
                RawData = rawBody
            };
            return result;
        }

        private string HmacSha256(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }
}