using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PetCenterAPI.DTOs;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(Guid orderId, decimal amount, string transactionRef, string clientIp, string orderInfo)
        {
            var tmnCode = _configuration["VnPay:TmnCode"];
            var hashSecret = _configuration["VnPay:HashSecret"];
            var baseUrl = _configuration["VnPay:BaseUrl"];
            var returnUrl = _configuration["VnPay:ReturnUrl"];
            var version = _configuration["VnPay:Version"];
            var command = _configuration["VnPay:Command"];
            var currCode = _configuration["VnPay:CurrCode"];
            var locale = _configuration["VnPay:Locale"];

            var vnpParams = new SortedList<string, string>
            {
                { "vnp_Version", version ?? "" },
                { "vnp_Command", command ?? "" },
                { "vnp_TmnCode", tmnCode ?? "" },
                { "vnp_Amount", ((long)(amount * 100)).ToString() },
                { "vnp_CurrCode", currCode ?? "" },
                { "vnp_TxnRef", transactionRef ?? "" },
                { "vnp_OrderInfo", orderInfo ?? "" },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", locale ?? "" },
                { "vnp_ReturnUrl", returnUrl ?? "" },
                { "vnp_IpAddr", clientIp ?? "" },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            var queryBuilder = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    queryBuilder.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            var queryString = queryBuilder.ToString().TrimEnd('&');
            var secureHash = HmacSha512(hashSecret ?? "", queryString).ToUpper();
            
            return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        public bool ValidateCallback(IQueryCollection query)
        {
            var vnp_SecureHash = query["vnp_SecureHash"].ToString();
            var hashSecret = _configuration["VnPay:HashSecret"];
            
            var vnpParams = new SortedList<string, string>();
            foreach (var kv in query)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    vnpParams.Add(kv.Key, kv.Value.ToString());
                }
            }
            
            var queryBuilder = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                queryBuilder.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
            var queryString = queryBuilder.ToString().TrimEnd('&');
            
            var checkSum = HmacSha512(hashSecret ?? "", queryString);
            
            return checkSum.Equals(vnp_SecureHash, StringComparison.OrdinalIgnoreCase);
        }

        public VnPayCallbackResult ParseCallback(IQueryCollection query)
        {
            var result = new VnPayCallbackResult
            {
                TransactionRef = query["vnp_TxnRef"].ToString(),
                GatewayTransactionNo = query["vnp_TransactionNo"].ToString(),
                ResponseCode = query["vnp_ResponseCode"].ToString(),
                Amount = decimal.TryParse(query["vnp_Amount"].ToString(), out var amount) ? amount / 100 : 0,
                BankCode = query["vnp_BankCode"].ToString(),
                IsSuccess = query["vnp_ResponseCode"].ToString() == "00",
                RawData = query.ToString()
            };
            return result;
        }

        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
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