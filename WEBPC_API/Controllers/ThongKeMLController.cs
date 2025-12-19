using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Services.Business;
using WEBPC_API.Models.ML;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeMLController : ControllerBase
    {
        private readonly ForecastService _forecastService;

        public ThongKeMLController(ForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        // API: Lấy dữ liệu vẽ biểu đồ chi tiêu & dự báo
        // GET: api/ThongKeML/bieu-do-chi-tieu?maKhachHang=1
        [HttpGet("bieu-do-chi-tieu")]
        public async Task<IActionResult> GetChartData([FromQuery] int maKhachHang)
        {
            try
            {
                if (maKhachHang <= 0)
                    return BadRequest(new { message = "Mã khách hàng không hợp lệ." });

                // 1. Lấy dữ liệu lịch sử (Chỉ tính đơn đã hoàn thành/giao thành công)
                var history = await _forecastService.GetHistoryDataAsync(maKhachHang);

                // Nếu khách chưa mua gì bao giờ
                if (history == null || history.Count == 0)
                {
                    return Ok(new
                    {
                        Message = "Khách hàng chưa có lịch sử giao dịch.",
                        LichSu = new List<object>(),
                        DuBao = new List<object>()
                    });
                }

                // 2. Chạy thuật toán dự báo (Dự đoán 7 ngày tiếp theo)
                var prediction = _forecastService.PredictNextDays(history, horizon: 7);

                // 3. Chuẩn bị dữ liệu trả về (Format lại cho đẹp để FE dễ vẽ)

                // Mảng lịch sử
                var historyChart = history.Select(x => new
                {
                    Ngay = x.NgayBan.ToString("dd/MM/yyyy"),
                    Tien = x.TongTien
                }).ToList();

                // Mảng dự báo (Nối tiếp ngày cuối cùng của lịch sử)
                var lastDate = history.Last().NgayBan;
                var forecastChart = new List<object>();

                if (prediction.ForecastedRevenue != null)
                {
                    for (int i = 0; i < prediction.ForecastedRevenue.Length; i++)
                    {
                        forecastChart.Add(new
                        {
                            Ngay = lastDate.AddDays(i + 1).ToString("dd/MM/yyyy"), // Ngày tương lai
                            TienDuDoan = Math.Max(0, prediction.ForecastedRevenue[i]) // Không lấy số âm
                        });
                    }
                }

                return Ok(new
                {
                    TongQuan = new
                    {
                        TongTienDaTieu = history.Sum(x => x.TongTien),
                        DuKienTuanToi = prediction.ForecastedRevenue?.Sum() ?? 0
                    },
                    BieuDo = new
                    {
                        LichSu = historyChart, // Đường nét liền (Quá khứ)
                        DuBao = forecastChart  // Đường nét đứt (Tương lai)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi Server: " + ex.Message });
            }
        }
    }
}