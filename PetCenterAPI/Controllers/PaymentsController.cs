using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Requests.Order;
using PetCenterAPI.Models;
using PetCenterAPI.Service.Interface;
using System.Text.Json;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {   
        private readonly ICheckoutService _checkoutService;
        private readonly IVnPayService _vnPayService;
        private readonly IMoMoService _moMoService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IAppointmentService _appointmentService;
        private readonly PetCenterContext _petCenterContext;

        public PaymentsController(
            ICheckoutService checkoutService,
            IVnPayService vnPayService,
            IMoMoService moMoService,
            ILogger<PaymentsController> logger,
            IAppointmentService appointmentService,
            PetCenterContext petCenterContext)
        {
            _checkoutService = checkoutService;
            _vnPayService = vnPayService;
            _moMoService = moMoService;
            _logger = logger;
            _appointmentService = appointmentService;
            _petCenterContext = petCenterContext;
        }

        // POST api/Payments/vnpay/create
        [HttpPost("vnpay/create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateVnPayPayment([FromBody] PlaceOnlineOrderDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                dto.PaymentMethod = "VNPAY";
                if (string.IsNullOrWhiteSpace(dto.CustomReturnUrl))
                {
                    dto.CustomReturnUrl = $"{Request.Scheme}://{Request.Host}/api/Payments/vnpay/return";
                }
                var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
                dto.ClientIpAddress = ip == "0.0.0.1" ? "127.0.0.1" : ip;
                var result = await _checkoutService.PlaceOnlineOrderAsync(dto);
                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPay] Error creating payment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET api/Payments/vnpay/return — browser redirect from VNPay
        [HttpGet("vnpay/return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            _logger.LogInformation("[VNPay] Return URL hit with query: {Query}", Request.QueryString);
            try
            {
                if (!_vnPayService.ValidateCallback(Request.Query))
                {
                    _logger.LogWarning("[VNPay] Invalid checksum on Return URL");
                    return BuildPaymentResultHtml(false, "", "Invalid checksum");
                }

                var cbResult = _vnPayService.ParseCallback(Request.Query);

                var payment = await _petCenterContext.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.TransactionRef == cbResult.TransactionRef);

                var processResult = await _checkoutService.ProcessPaymentCallbackAsync(
                    cbResult.TransactionRef,
                    cbResult.GatewayTransactionNo,
                    cbResult.ResponseCode,
                    cbResult.BankCode,
                    cbResult.Amount,
                    cbResult.RawData,
                    cbResult.IsSuccess);

                return BuildPaymentResultHtml(processResult.Success, processResult.OrderId?.ToString() ?? "", processResult.Message ?? "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPay] Error processing Return URL");
                return BuildPaymentResultHtml(false, "", ex.Message);
            }
        }

        // GET api/Payments/vnpay/ipn — server-to-server from VNPay
        [HttpGet("vnpay/ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIpn()
        {
            _logger.LogInformation("[VNPay] IPN hit with query: {Query}", Request.QueryString);
            try
            {
                if (!_vnPayService.ValidateCallback(Request.Query))
                {
                    _logger.LogWarning("[VNPay] Invalid checksum on IPN");
                    return Ok(new { RspCode = "97", Message = "Invalid checksum" });
                }

                var cbResult = _vnPayService.ParseCallback(Request.Query);
                var processResult = await _checkoutService.ProcessPaymentCallbackAsync(
                    cbResult.TransactionRef,
                    cbResult.GatewayTransactionNo,
                    cbResult.ResponseCode,
                    cbResult.BankCode,
                    cbResult.Amount,
                    cbResult.RawData,
                    cbResult.IsSuccess);

                if (processResult.Success)
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });

                if (processResult.Message != null && processResult.Message.Contains("already been processed"))
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });

                return Ok(new { RspCode = "02", Message = processResult.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPay] Error processing IPN");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        // POST api/Payments/momo/create
        [HttpPost("momo/create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateMoMoPayment([FromBody] PlaceOnlineOrderDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                dto.PaymentMethod = "MOMO";
                if (string.IsNullOrWhiteSpace(dto.CustomReturnUrl))
                {
                    dto.CustomReturnUrl = $"{Request.Scheme}://{Request.Host}/api/Payments/momo/return";
                }
                var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
                dto.ClientIpAddress = ip == "0.0.0.1" ? "127.0.0.1" : ip;
                var result = await _checkoutService.PlaceOnlineOrderAsync(dto);
                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MoMo] Error creating payment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET api/Payments/momo/return — browser redirect from MoMo
        [HttpGet("momo/return")]
        [AllowAnonymous]
        public async Task<IActionResult> MoMoReturn()
        {
            _logger.LogInformation("[MoMo] Return URL hit with query: {Query}", Request.QueryString);
            try
            {
                var resultCode = Request.Query["resultCode"].FirstOrDefault();
                var transactionRef = Request.Query["orderId"].FirstOrDefault() ?? string.Empty;
                var message = Request.Query["message"].FirstOrDefault() ?? "";
                var transId = Request.Query["transId"].FirstOrDefault() ?? "";
                var amountStr = Request.Query["amount"].FirstOrDefault();

                var success = resultCode == "0";
                decimal.TryParse(amountStr, out var amount);

                var payment = await _petCenterContext.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.TransactionRef == transactionRef);

                var processResult = await _checkoutService.ProcessPaymentCallbackAsync(
                    transactionRef,
                    transId,
                    resultCode ?? "99",
                    string.Empty,
                    amount,
                    Request.QueryString.ToString(),
                    success);

                return BuildPaymentResultHtml(processResult.Success, processResult.OrderId?.ToString() ?? "", processResult.Message ?? "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MoMo] Error processing Return URL");
                return BuildPaymentResultHtml(false, "", ex.Message);
            }
        }

        private ContentResult BuildPaymentResultHtml(bool isSuccess, string orderId, string message)
        {
            var statusColor = isSuccess ? "#0d9488" : "#ef4444";
            var iconBg = isSuccess ? "#e6fffa" : "#fef2f2";
            var iconSymbol = isSuccess ? "✓" : "✕";
            var title = isSuccess ? "Thanh toán thành công!" : "Thanh toán không thành công";
            var desc = isSuccess
                ? $"Giao dịch cho đơn hàng #{orderId} đã được xác nhận thành công trên hệ thống Pet Center."
                : $"Rất tiếc, giao dịch chưa hoàn tất: {message}";

            var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>Xác nhận thanh toán - Pet Center</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; align-items: center; justify-content: center; height: 100vh; margin: 0; background: #f8fafc; text-align: center; }}
        .card {{ background: white; padding: 40px 30px; border-radius: 20px; box-shadow: 0 10px 25px rgba(0,0,0,0.08); max-width: 400px; width: 90%; }}
        .icon {{ width: 72px; height: 72px; background: {iconBg}; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 20px; color: {statusColor}; font-size: 36px; font-weight: bold; line-height: 1; }}
        h2 {{ margin: 0 0 10px; color: #0f172a; font-size: 22px; }}
        p {{ color: #64748b; font-size: 14px; line-height: 1.5; margin: 0 0 20px; }}
        .order-tag {{ display: inline-block; background: #f1f5f9; color: #334155; padding: 6px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; margin-bottom: 20px; }}
        .btn {{ display: block; width: 100%; box-sizing: border-box; background: #0d9488; color: white; padding: 14px 20px; border-radius: 12px; text-decoration: none; font-weight: bold; font-size: 15px; margin-bottom: 10px; border: none; cursor: pointer; }}
        .btn-secondary {{ background: #f1f5f9; color: #334155; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""icon"">{iconSymbol}</div>
        <h2>{title}</h2>
        {(string.IsNullOrEmpty(orderId) ? "" : $"<div class=\"order-tag\">Mã đơn hàng: #{orderId}</div>")}
        <p>{desc}</p>
        <p style=""font-size:12px; color:#94a3b8;"">Bạn có thể đóng cửa sổ này và quay lại ứng dụng Pet Center Mobile.</p>
        <a href=""https://localhost:7010/Checkout/PaymentReturn?success={isSuccess.ToString().ToLower()}&orderId={orderId}&message={Uri.EscapeDataString(message)}"" class=""btn btn-secondary"">Chuyển sang trang Web Pet Center</a>
    </div>
</body>
</html>";

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = html
            };
        }

        // POST api/Payments/momo/ipn — server-to-server from MoMo
        [HttpPost("momo/ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> MoMoIpn()
        {
            _logger.LogInformation("[MoMo] IPN received");
            try
            {
                using var reader = new StreamReader(Request.Body);
                var rawBody = await reader.ReadToEndAsync();
                _logger.LogInformation("[MoMo] IPN body: {Body}", rawBody);

                var json = JsonSerializer.Deserialize<JsonElement>(rawBody);
                var signature = json.GetProperty("signature").GetString() ?? "";

                if (!_moMoService.ValidateCallback(rawBody, signature))
                {
                    _logger.LogWarning("[MoMo] Invalid signature on IPN");
                    return NoContent();
                }

                var cbResult = _moMoService.ParseCallback(json, rawBody);
                await _checkoutService.ProcessPaymentCallbackAsync(
                    cbResult.TransactionRef,
                    cbResult.GatewayTransactionNo,
                    cbResult.ResponseCode,
                    string.Empty, // MoMo doesn't provide BankCode
                    cbResult.Amount,
                    cbResult.RawData,
                    cbResult.IsSuccess);

                return NoContent(); // MoMo expects 204
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MoMo] Error processing IPN");
                return NoContent();
            }
        }
    }
}