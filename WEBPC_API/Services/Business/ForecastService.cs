using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using WEBPC_API.Data;
using WEBPC_API.Models.Enums; // Namespace chứa Enum TrangThaiDonHang
using WEBPC_API.Models.ML;    // Namespace chứa RevenueData, SpendingAnalysisData

namespace WEBPC_API.Services.Business
{
    public class ForecastService
    {
        private readonly DataContext _context;
        private readonly MLContext _mlContext;

        public ForecastService(DataContext context)
        {
            _context = context;
            _mlContext = new MLContext();
        }

        // =========================================================
        // HÀM 1: Lấy dữ liệu lịch sử để chạy ML (Chỉ lấy đơn Hoàn Thành)
        // =========================================================
        public async Task<List<RevenueData>> GetHistoryDataAsync(int maKhachHang)
        {
            // [FIX LỖI]: Dùng đúng tên Enum "HoanThanh"
            string sHoanThanh = TrangThaiDonHang.HoanThanh.ToString();

            // Lấy dữ liệu từ DB
            var rawData = await _context.DonHang
                .Where(dh => dh.maKhachHang == maKhachHang && dh.trangThai == sHoanThanh)
                .GroupBy(dh => dh.ngayDat.Date)
                .Select(g => new
                {
                    Ngay = g.Key,
                    TongTien = (float)g.Sum(x => x.tongTien)
                })
                .OrderBy(x => x.Ngay)
                .ToListAsync();

            if (!rawData.Any()) return new List<RevenueData>();

            // Lấp đầy ngày trống (Logic bắt buộc cho Time Series)
            var result = new List<RevenueData>();
            var startDate = rawData.First().Ngay;
            var endDate = DateTime.Now.Date;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var salesOnDate = rawData.FirstOrDefault(x => x.Ngay == date);
                result.Add(new RevenueData
                {
                    NgayBan = date,
                    TongTien = salesOnDate != null ? salesOnDate.TongTien : 0
                });
            }

            return result;
        }

        // =========================================================
        // HÀM 2: Lấy dữ liệu tổng hợp cho Chatbot (Phân loại chi tiết)
        // =========================================================
        public async Task<SpendingAnalysisData> GetSpendingForChatbotAsync(int maKhachHang)
        {
            // 1. Lấy đơn hàng + Chi tiết đơn hàng để biết tên sản phẩm
            var allOrders = await _context.DonHang
                .Include(dh => dh.ChiTietDonHangs) // Join bảng chi tiết
                .ThenInclude(ct => ct.SanPham)     // Join bảng sản phẩm để lấy tên
                .Where(dh => dh.maKhachHang == maKhachHang)
                .OrderByDescending(dh => dh.ngayDat)
                .ToListAsync();

            var analysis = new SpendingAnalysisData();

            // Các biến so sánh trạng thái (như cũ)
            string sHoanThanh = TrangThaiDonHang.HoanThanh.ToString();
            string sHuy = TrangThaiDonHang.Huy.ToString();
            string sChoHoanTien = TrangThaiDonHang.ChoHoanTien.ToString();
            var pendingStatuses = new List<string> {
                TrangThaiDonHang.ChoXacNhan.ToString(), TrangThaiDonHang.ChoThanhToan.ToString(),
                TrangThaiDonHang.DaThanhToan.ToString(), TrangThaiDonHang.DangGiao.ToString()
            };

            // 2. Tính toán thống kê
            foreach (var order in allOrders)
            {
                if (order.trangThai == sHoanThanh)
                {
                    analysis.TongTienDaChi += (float)order.tongTien;
                    analysis.SoDonThanhCong++;
                }
                else if (order.trangThai == sHuy || order.trangThai == sChoHoanTien)
                {
                    analysis.TongTienDaHuy += (float)order.tongTien;
                    analysis.SoDonHuy++;
                }
                else if (pendingStatuses.Contains(order.trangThai))
                {
                    analysis.TongTienDangCho += (float)order.tongTien;
                }
            }

            // 3. Lấy lịch sử mua (Kèm tên sản phẩm để Bot biết)
            analysis.DonHangGanDay = allOrders.Take(5).Select(x => new DonHangTomTat
            {
                Ngay = x.ngayDat,
                TrangThai = x.trangThai,
                SoTien = (float)x.tongTien,
                // Lấy tên sản phẩm đầu tiên trong đơn làm đại diện
                TenSanPhamChinh = x.ChiTietDonHangs.FirstOrDefault()?.SanPham?.TenSanPham ?? "Linh kiện PC"
            }).ToList();

            // 4. [QUAN TRỌNG] Lấy danh sách sản phẩm để gợi ý (Lấy 20 món mới nhất/bán chạy)
            // Chỉ lấy hàng còn trong kho (SoLuongTon > 0) nếu em có cột đó, ở đây thầy lấy đơn giản
            var products = await _context.SanPhams
                .Include(sp => sp.DanhMuc)
                .OrderByDescending(sp => sp.MaSanPham) // Hoặc logic bán chạy
                .Take(20)
                .Select(sp => new SanPhamGoiY
                {
                    MaSanPham = sp.MaSanPham, // [THÊM DÒNG NÀY]
                    TenSanPham = sp.TenSanPham,
                    Gia = (float)sp.GiaBan,
                    DanhMuc = sp.DanhMuc.TenDanhMuc
                })
                .ToListAsync();

            analysis.SanPhamShopDangBan = products;

            return analysis;
        }

        // =========================================================
        // HÀM 3: Chạy thuật toán dự báo (ML.NET) - GIỮ NGUYÊN
        // =========================================================
        public RevenuePrediction PredictNextDays(List<RevenueData> historyData, int horizon = 7)
        {
            if (historyData.Count < 10)
            {
                float avg = historyData.Any() ? historyData.Average(x => x.TongTien) : 0;
                return new RevenuePrediction
                {
                    ForecastedRevenue = Enumerable.Repeat(avg, horizon).ToArray()
                };
            }

            var dataView = _mlContext.Data.LoadFromEnumerable(historyData);

            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "Score",
                inputColumnName: nameof(RevenueData.TongTien),
                windowSize: 7,
                seriesLength: historyData.Count,
                trainSize: historyData.Count,
                horizon: horizon,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBound",
                confidenceUpperBoundColumn: "UpperBound");

            var model = pipeline.Fit(dataView);
            var forecastingEngine = model.CreateTimeSeriesEngine<RevenueData, RevenuePrediction>(_mlContext);
            var prediction = forecastingEngine.Predict();

            return prediction;
        }
    }
}