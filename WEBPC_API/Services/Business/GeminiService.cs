using System.Text;
using System.Text.Json;
using System.Net;
using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Services.Business;
using WEBPC_API.Models.ML;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class GeminiService
    {
        private readonly ForecastService _forecastService;
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        // [MỚI] Thêm 2 ông này
        private readonly IKhuyenMaiRepository _khuyenMaiRepo;
        private readonly IDanhMucRepository _danhMucRepo;

        // DANH SÁCH MODEL DỰ PHÒNG (Giữ nguyên)
        private readonly string[] _backupModels = new[]
        {
            "gemini-2.5-flash", "gemini-2.0-flash", "gemini-1.5-flash", "gemini-flash-latest"
        };

        // [MỚI] Cập nhật Constructor
        public GeminiService(
            ForecastService forecastService,
            DataContext context,
            IConfiguration configuration,
            HttpClient httpClient,
            IKhuyenMaiRepository khuyenMaiRepo, // Tiêm vào
            IDanhMucRepository danhMucRepo      // Tiêm vào
        )
        {
            _forecastService = forecastService;
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
            _khuyenMaiRepo = khuyenMaiRepo;     // Gán
            _danhMucRepo = danhMucRepo;         // Gán
        }

        public async Task<string> ChatWithGemini(int maKhachHang, string userMessage)
        {
            // 1. XỬ LÝ PHIÊN CHAT (GIỮ NGUYÊN CODE CŨ)
            // -----------------------------------------------------------
            var phienChat = await _context.PhienChats.Include(p => p.ChiTietPhienChats)
                .FirstOrDefaultAsync(p => p.MaKhachHang == maKhachHang);
            // ... (Đoạn logic kiểm tra 10 phút, xóa, tạo mới... em giữ nguyên như cũ nhé) ...
            // Thầy viết tắt đoạn này để tập trung vào phần mới bên dưới
            List<ChiTietPhienChat> lichSuChat = new List<ChiTietPhienChat>();
            if (phienChat != null)
            {
                if ((DateTime.Now - phienChat.ThoiGianCapNhat).TotalMinutes > 10)
                {
                    _context.PhienChats.Remove(phienChat);
                    await _context.SaveChangesAsync();
                    phienChat = null;
                }
                else lichSuChat = phienChat.ChiTietPhienChats.OrderBy(x => x.ThoiGian).ToList();
            }
            if (phienChat == null)
            {
                phienChat = new PhienChat { MaKhachHang = maKhachHang, ThoiGianCapNhat = DateTime.Now };
                _context.PhienChats.Add(phienChat);
                await _context.SaveChangesAsync();
            }
            // -----------------------------------------------------------


            // 2. CHUẨN BỊ DỮ LIỆU (NÂNG CẤP MẠNH MẼ)
            // -----------------------------------------------------------

            // A. Dữ liệu tài chính & Sản phẩm (Cũ)
            SpendingAnalysisData data;
            try { data = await _forecastService.GetSpendingForChatbotAsync(maKhachHang); }
            catch { data = new SpendingAnalysisData(); }

            // B. [MỚI] Lấy danh sách Khuyến Mãi đang hiệu lực (Ngày KT > Hiện tại)
            var allPromotions = await _khuyenMaiRepo.GetAllAsync(); // Giả sử em có hàm GetAll
            var activePromos = allPromotions
                .Where(k => k.NgayKetThuc > DateTime.Now && k.NgayBatDau <= DateTime.Now)
                .Select(k => $"- MÃ: [{k.MaCodeKM}] ({k.TenChuongTrinh}): Giảm {k.GiaTriGiam}% (Tối đa {k.GiamToiDa}đ). Đơn tối thiểu: {k.DonHangToiThieu}đ")
                .ToList();
            string promoContext = activePromos.Any() ? string.Join("\n", activePromos) : "Hiện không có khuyến mãi nào.";

            // C. [MỚI] Lấy danh sách Danh Mục
            var categories = await _danhMucRepo.GetAllAsync();
            string categoryContext = string.Join(", ", categories.Select(c => c.TenDanhMuc));


            // Helper format tiền
            string Fmt(float amount) => amount == 0 ? "0đ" : string.Format("{0:N0}đ", amount);
            string frontendUrl = _configuration["FrontendSettings:BaseUrl"] ?? "http://localhost:44384";
            frontendUrl = frontendUrl.TrimEnd('/');


            // 3. VIẾT PROMPT (BỔ SUNG THÔNG TIN VÀO ĐÂY)
            // -----------------------------------------------------------
            string chatHistoryText = "";
            if (lichSuChat.Any())
            {
                chatHistoryText = "LỊCH SỬ CHAT TRƯỚC ĐÓ:\n" +
                                  string.Join("\n", lichSuChat.Select(x => $"User: {x.CauHoi}\nBot: {x.CauTraLoi}"));
            }

            // List sản phẩm (Có kèm tên Danh Mục để AI lọc)
            string productContext = string.Join("\n", data.SanPhamShopDangBan.Select(p =>
                $"- [{p.DanhMuc}] {p.TenSanPham} (ID: {p.MaSanPham}) - Giá: {Fmt(p.Gia)}"
            ));

            string prompt = $@"
            Bạn là Trợ lý AI cao cấp của cửa hàng 'WEBPC'. Bạn đóng vai nhân viên bán hàng xuất sắc.

            1. DỮ LIỆU CỬA HÀNG (Đây là thông tin thực tế, hãy dùng nó để trả lời):
            
            [DANH MỤC SẢN PHẨM HIỆN CÓ]
            {categoryContext}

            [CHƯƠNG TRÌNH KHUYẾN MÃI ĐANG CHẠY (HOT)]
            {promoContext}

            [DANH SÁCH SẢN PHẨM CHI TIẾT]
            {productContext}

            2. HỒ SƠ KHÁCH HÀNG:
            - Tổng chi tiêu: {Fmt(data.TongTienDaChi)}.

            3. LỊCH SỬ TRÒ CHUYỆN:
            {chatHistoryText}

            4. CÂU HỎI MỚI CỦA KHÁCH: ""{userMessage}""

            YÊU CẦU TRẢ LỜI (QUAN TRỌNG):
            - Nếu khách hỏi khuyến mãi/ưu đãi: Hãy giới thiệu các mã trong mục [CHƯƠNG TRÌNH KHUYẾN MÃI].
            - Nếu khách hỏi về một danh mục (Ví dụ: 'Có bán Ram không?'): Hãy liệt kê các sản phẩm có tag [RAM] trong mục [DANH SÁCH SẢN PHẨM].
            - Nếu gợi ý sản phẩm cụ thể: BẮT BUỘC dùng định dạng link: [Tên Sản Phẩm]({frontendUrl}/Product/Detail/{{id}})
            - Giọng điệu: Nhiệt tình, chuyên nghiệp, khuyến khích khách mua hàng (Upsell).
            ";

            // 4. GỌI API GEMINI (FAILOVER LOGIC - GIỮ NGUYÊN)
            // -----------------------------------------------------------
            string botAnswer = "";
            string lastError = "";
            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrEmpty(apiKey)) return "Lỗi: Chưa cấu hình API Key.";

            var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            string jsonPayload = JsonSerializer.Serialize(payload);

            foreach (var modelName in _backupModels)
            {
                try
                {
                    var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";
                    using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var respString = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(respString);
                        if (doc.RootElement.TryGetProperty("candidates", out var c) && c.GetArrayLength() > 0)
                        {
                            botAnswer = c[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                            break;
                        }
                    }
                    else
                    {
                        lastError = $"Model {modelName} failed ({response.StatusCode}). ";
                    }
                }
                catch (Exception ex)
                {
                    lastError += $"Model {modelName} exception: {ex.Message}. ";
                }
            }

            if (string.IsNullOrEmpty(botAnswer)) botAnswer = $"Xin lỗi, hệ thống đang bận. (Debug: {lastError})";

            // 5. LƯU DATABASE (GIỮ NGUYÊN)
            if (!string.IsNullOrEmpty(botAnswer))
            {
                var tinNhanMoi = new ChiTietPhienChat
                {
                    MaPhien = phienChat.MaPhien,
                    CauHoi = userMessage,
                    CauTraLoi = botAnswer,
                    ThoiGian = DateTime.Now
                };
                _context.ChiTietPhienChats.Add(tinNhanMoi);
                phienChat.ThoiGianCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return botAnswer;
        }

        // HÀM LẤY LỊCH SỬ (GIỮ NGUYÊN)
        public async Task<List<ChatMessageResponse>> GetChatHistoryAsync(int maKhachHang)
        {
            var phienChat = await _context.PhienChats
                .Include(p => p.ChiTietPhienChats)
                .FirstOrDefaultAsync(p => p.MaKhachHang == maKhachHang);

            if (phienChat == null) return new List<ChatMessageResponse>();

            if ((DateTime.Now - phienChat.ThoiGianCapNhat).TotalMinutes > 10)
            {
                _context.PhienChats.Remove(phienChat);
                await _context.SaveChangesAsync();
                return new List<ChatMessageResponse>();
            }

            var history = new List<ChatMessageResponse>();
            foreach (var item in phienChat.ChiTietPhienChats.OrderBy(x => x.ThoiGian))
            {
                history.Add(new ChatMessageResponse { Role = "user", Content = item.CauHoi, Time = item.ThoiGian });
                history.Add(new ChatMessageResponse { Role = "model", Content = item.CauTraLoi, Time = item.ThoiGian.AddSeconds(1) });
            }
            return history;
        }
    }
}