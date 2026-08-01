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

                // 1. Tìm bản ghi Payment để kiểm tra loại giao dịch (Appointment hay Order)
                var payment = await _petCenterContext.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.TransactionRef == cbResult.TransactionRef);

                // 2. Xử lý logic cập nhật DB (ProcessPaymentCallbackAsync đã phân nhánh ở bước trước)
                var processResult = await _checkoutService.ProcessPaymentCallbackAsync(
                    cbResult.TransactionRef,
                    cbResult.GatewayTransactionNo,
                    cbResult.ResponseCode,
                    cbResult.BankCode,
                    cbResult.Amount,
                    cbResult.RawData,
                    cbResult.IsSuccess);

                // 3. Phân nhánh Redirect dựa vào Payment Entity
                if (payment != null && payment.AppointmentId.HasValue && payment.AppointmentId.Value != Guid.Empty)
                {
                    // Direct sang Client Action: /Appointment/PaymentReturn
                    var appointmentRedirectUrl = $"https://localhost:7010/Appointment/PaymentReturn" +
                        $"?success={processResult.Success}" +
                        $"&appointmentId={payment.AppointmentId}" +
                        $"&message={Uri.EscapeDataString(processResult.Message ?? "")}";

                    return Redirect(appointmentRedirectUrl);
                }
                else
                {
                    // Direct sang Client Action: /Checkout/PaymentReturn (Order)
                    var orderRedirectUrl = $"https://localhost:7010/Checkout/PaymentReturn" +
                        $"?success={processResult.Success}" +
                        $"&orderId={processResult.OrderId}" +
                        $"&message={Uri.EscapeDataString(processResult.Message ?? "")}";

                    return Redirect(orderRedirectUrl);
                }
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
                var transactionRef = Request.Query["orderId"].FirstOrDefault() ?? string.Empty; // MoMo's orderId is our TransactionRef
                var message = Request.Query["message"].FirstOrDefault() ?? "";
                var transId = Request.Query["transId"].FirstOrDefault() ?? "";
                var amountStr = Request.Query["amount"].FirstOrDefault();

                var success = resultCode == "0";
                decimal.TryParse(amountStr, out var amount);

                // 1. Kiểm tra bản ghi Payment trong DB để xác định loại giao dịch
                var payment = await _petCenterContext.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.TransactionRef == transactionRef);

                // 2. Xử lý cập nhật trạng thái thanh toán & đặt lịch/đơn hàng
                var processResult = await _checkoutService.ProcessPaymentCallbackAsync(
                    transactionRef,
                    transId,
                    resultCode ?? "99",
                    string.Empty, // MoMo doesn't provide BankCode
                    amount,
                    Request.QueryString.ToString(),
                    success);

                // 3. Phân nhánh Redirect dựa trên Payment Entity
                if (payment != null && payment.AppointmentId.HasValue && payment.AppointmentId.Value != Guid.Empty)
                {
                    // Redirect về Client Appointment Result
                    var appointmentRedirectUrl = $"https://localhost:7010/Appointment/PaymentReturn" +
                        $"?success={processResult.Success}" +
                        $"&appointmentId={payment.AppointmentId}" +
                        $"&message={Uri.EscapeDataString(processResult.Message ?? "")}";

                    return Redirect(appointmentRedirectUrl);
                }

                // Redirect về Client Order Result (mặc định cho Order)
                var orderRedirectUrl = $"https://localhost:7010/Checkout/PaymentReturn" +
                    $"?success={processResult.Success}" +
                    $"&orderId={processResult.OrderId}" +
                    $"&message={Uri.EscapeDataString(processResult.Message ?? "")}";

                return Redirect(orderRedirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MoMo] Error processing Return URL");
                // Khi xảy ra Exception, ưu tiên fallback về Appointment/PaymentReturn nếu thất bại
                return Redirect($"https://localhost:7010/Appointment/PaymentReturn?success=false&message={Uri.EscapeDataString(ex.Message)}");
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