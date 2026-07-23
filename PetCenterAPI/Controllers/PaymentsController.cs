using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetCenterAPI.DTOs.Requests.Order;
using PetCenterAPI.Service.Interface;

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

        public PaymentsController(
            ICheckoutService checkoutService,
            IVnPayService vnPayService,
            IMoMoService moMoService,
            ILogger<PaymentsController> logger)
        {
            _checkoutService = checkoutService;
            _vnPayService = vnPayService;
            _moMoService = moMoService;
            _logger = logger;
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
                    return Redirect("https://localhost:7010/Checkout/PaymentReturn?success=false&message=Invalid+checksum");
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

                var redirectUrl = $"https://localhost:7010/Checkout/PaymentReturn?success={processResult.Success}&orderId={processResult.OrderId}&message={Uri.EscapeDataString(processResult.Message ?? "")}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPay] Error processing Return URL");
                return Redirect($"https://localhost:7010/Checkout/PaymentReturn?success=false&message={Uri.EscapeDataString(ex.Message)}");
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

                // If the order was already processed (idempotency), still return success to VNPay
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
                // MoMo sends parameters in query string for redirect
                var resultCode = Request.Query["resultCode"].FirstOrDefault();
                var orderId = Request.Query["orderId"].FirstOrDefault();
                var message = Request.Query["message"].FirstOrDefault() ?? "";
                var transId = Request.Query["transId"].FirstOrDefault() ?? "";
                var amountStr = Request.Query["amount"].FirstOrDefault();
                
                var success = resultCode == "0";
                decimal.TryParse(amountStr, out var amount);

                var processResult = await _checkoutService.ProcessPaymentCallbackAsync(
                    orderId ?? string.Empty,
                    transId,
                    resultCode ?? "99",
                    string.Empty, // MoMo doesn't provide BankCode
                    amount,
                    Request.QueryString.ToString(),
                    success);

                var redirectUrl = $"https://localhost:7010/Checkout/PaymentReturn?success={processResult.Success}&orderId={processResult.OrderId}&message={Uri.EscapeDataString(processResult.Message)}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MoMo] Error processing Return URL");
                return Redirect($"https://localhost:7010/Checkout/PaymentReturn?success=false&message={Uri.EscapeDataString(ex.Message)}");
            }
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