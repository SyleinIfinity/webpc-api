using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly IDonHangRepository _repo;

        public DonHangController(IDonHangRepository repo)
        {
            _repo = repo;
        }

        // 1. GET: api/donhang (Dành cho Nhân viên xem danh sách)
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var list = await _repo.GetAllAsync();
            return Ok(list);
        }

        // 2. GET: api/donhang/{id} (Xem chi tiết 1 đơn)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
            return Ok(order);
        }

        // 3. POST: api/donhang (Dành cho Khách hàng tạo đơn - Test nhanh)
        // Lưu ý: Đây là API tạo đơn đơn giản để em test luồng Payment/Refund
        [HttpPost]
        public async Task<IActionResult> CreateOrderTest([FromBody] TaoDonHangRequest request)
        {
            // Tạo mã đơn hàng ngẫu nhiên (VD: DH-8392)
            string maCode = "DH-" + new Random().Next(1000, 9999);

            var donHang = new DonHang
            {
                maCodeDonHang = maCode,
                maKhachHang = request.MaKhachHang,
                ngayDat = DateTime.Now,
                tongTien = request.TongTien,
                trangThai = "ChoXacNhan",

                // Các thông tin giao hàng giả lập (hoặc lấy từ request nếu muốn kỹ)
                diaChiGiaoHang = "123 Đường Test, Đà Nẵng",
                soDienThoaiGiao = "0905123456",
                nguoiNhan = "Khách Test",
                phiVanChuyen = 0
            };

            await _repo.AddAsync(donHang);

            return Ok(new
            {
                message = "Tạo đơn hàng thành công!",
                maDonHang = donHang.maDonHang, // ID số (Dùng cho API Refund/Reject)
                maCode = donHang.maCodeDonHang // Mã Code (Dùng cho Webhook Casso)
            });
        }

        [HttpGet("history/{maKhachHang}")]
        public async Task<IActionResult> GetOrdersByCustomer(int maKhachHang)
        {
            var list = await _repo.GetByKhachHangIdAsync(maKhachHang);

            if (list == null || !list.Any())
            {
                return Ok(new List<DonHang>()); // Trả về danh sách rỗng thay vì 404
            }

            return Ok(list);
        }
    }

    // Class DTO nhỏ để hứng dữ liệu tạo đơn nhanh
    public class TaoDonHangRequest
    {
        public int MaKhachHang { get; set; }
        public decimal TongTien { get; set; }
    }


}