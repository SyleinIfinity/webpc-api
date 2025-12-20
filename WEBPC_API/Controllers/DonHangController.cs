using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        // Inject Service, không Inject Repository trực tiếp
        private readonly IDonHangService _donHangService;

        public DonHangController(IDonHangService donHangService)
        {
            _donHangService = donHangService;
        }

        // POST: api/DonHang/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] TaoDonHangRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newOrder = await _donHangService.CreateOrderAsync(request);

                // Trả về thông tin đơn hàng vừa tạo
                return Ok(new
                {
                    success = true,
                    message = "Tạo đơn hàng thành công",
                    maDonHang = newOrder.maDonHang,
                    maCode = newOrder.maCodeDonHang,
                    tongTien = newOrder.tongTien
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi (VD: Hết hàng, Giỏ hàng trống...)
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // [MỚI] GET: api/DonHang (Lấy tất cả - Dành cho Admin)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _donHangService.GetAllOrdersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // [MỚI] GET: api/DonHang/{id} (Lấy chi tiết 1 đơn)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _donHangService.GetOrderByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // [MỚI] GET: api/DonHang/customer/{customerId} (Lấy lịch sử đơn của khách)
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            try
            {
                var result = await _donHangService.GetOrdersByCustomerAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _donHangService.CancelOrderAsync(id, request);
                return Ok(new
                {
                    success = true,
                    message = "Hủy đơn hàng thành công."
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi 400 Bad Request nếu vi phạm logic (ví dụ: hủy đơn đã thanh toán)
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // [MỚI] CÁC API DUYỆT VÀ TỪ CHỐI ĐƠN HÀNG
        // ============================================================

        // 1. DUYỆT ĐƠN HÀNG
        // URL: PUT api/DonHang/approve/10
        [HttpPut("approve/{id}")]
        // [Authorize(Roles = "Admin,NhanVien")] // Bỏ comment nếu cần bảo mật
        public async Task<IActionResult> ApproveOrder(int id)
        {
            try
            {
                var result = await _donHangService.ApproveOrderAsync(id);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Đã duyệt đơn hàng #{id} thành công. Trạng thái mới: Đang giao hàng."
                    });
                }

                return BadRequest(new { success = false, message = "Duyệt đơn hàng thất bại." });
            }
            catch (KeyNotFoundException ex)
            {
                // Trả về 404 nếu không tìm thấy ID
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Trả về 400 nếu vi phạm logic (VD: Chưa thanh toán VietQR, sai trạng thái)
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Trả về 500 lỗi hệ thống
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 2. TỪ CHỐI ĐƠN HÀNG
        // URL: PUT api/DonHang/reject/10
        // Body: { "lyDoTuChoi": "Hết hàng kho Hà Nội" }
        [HttpPut("reject/{id}")]
        // [Authorize(Roles = "Admin,NhanVien")] // Bỏ comment nếu cần bảo mật
        public async Task<IActionResult> RejectOrder(int id, [FromBody] RejectOrderRequest request)
        {
            try
            {
                // Validate dữ liệu đầu vào
                if (request == null || string.IsNullOrWhiteSpace(request.LyDoTuChoi))
                {
                    return BadRequest(new { success = false, message = "Vui lòng cung cấp lý do từ chối (LyDoTuChoi)." });
                }

                // Gọi Service xử lý logic
                var result = await _donHangService.RejectOrderAsync(id, request);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Đã từ chối đơn hàng #{id} thành công. Đã hoàn lại số lượng tồn kho."
                    });
                }

                return BadRequest(new { success = false, message = "Từ chối đơn hàng thất bại." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Trả về 400 nếu vi phạm logic (VD: Đơn VietQR đã thanh toán rồi thì không cho từ chối)
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("confirm-received/{id}")]
        [Authorize(Roles = "KhachHang")] // Chỉ khách hàng mới được gọi
        public async Task<IActionResult> ConfirmReceived(int id)
        {
            try
            {
                // Lấy ID khách từ Token
                var maKhachHang = int.Parse(User.FindFirst("MaKhachHang")?.Value);

                var result = await _donHangService.XacNhanNhanHangAsync(id, maKhachHang);

                if (result)
                {
                    return Ok(new { message = "Xác nhận nhận hàng thành công!" });
                }
                else
                {
                    return BadRequest(new { message = "Không thể xác nhận. Đơn hàng không ở trạng thái đang vận chuyển hoặc không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}