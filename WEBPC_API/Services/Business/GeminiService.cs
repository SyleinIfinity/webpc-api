using System.Text;
using System.Text.Json;
using System.Net;
using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data; // Để gọi DataContext
using WEBPC_API.Services.Business;
using WEBPC_API.Models.ML;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Business
{
    public class GeminiService
    {
        private readonly ForecastService _forecastService;
        private readonly DataContext _context; // [MỚI] Cần DB để lưu chat
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GeminiService(ForecastService forecastService, DataContext context, IConfiguration configuration, HttpClient httpClient)
        {
            _forecastService = forecastService;
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> ChatWithGemini(int maKhachHang, string userMessage)
        {
            // 1. XỬ LÝ PHIÊN CHAT & LOGIC 10 PHÚT
            // -----------------------------------------------------------
            var phienChat = await _context.PhienChats
                .Include(p => p.ChiTietPhienChats)
                .FirstOrDefaultAsync(p => p.MaKhachHang == maKhachHang);

            List<ChiTietPhienChat> lichSuChat = new List<ChiTietPhienChat>();

            if (phienChat != null)
            {
                // Kiểm tra thời gian: Nếu quá 10 phút không chat -> Xóa phiên cũ (Reset)
                if ((DateTime.Now - phienChat.ThoiGianCapNhat).TotalMinutes > 10)
                {
                    _context.PhienChats.Remove(phienChat);
                    await _context.SaveChangesAsync();
                    phienChat = null; // Coi như chưa có
                }
                else
                {
                    // Nếu còn hạn: Lấy lịch sử để AI nhớ
                    lichSuChat = phienChat.ChiTietPhienChats.OrderBy(x => x.ThoiGian).ToList();
                }
            }

            // Nếu chưa có phiên (hoặc vừa xóa), tạo mới
            if (phienChat == null)
            {
                phienChat = new PhienChat { MaKhachHang = maKhachHang, ThoiGianCapNhat = DateTime.Now };
                _context.PhienChats.Add(phienChat);
                await _context.SaveChangesAsync();
            }
            // -----------------------------------------------------------


            // 2. LẤY DỮ LIỆU TÀI CHÍNH & SẢN PHẨM (Code cũ)
            SpendingAnalysisData data;
            try { data = await _forecastService.GetSpendingForChatbotAsync(maKhachHang); }
            catch { data = new SpendingAnalysisData(); }

            string Fmt(float amount) => amount == 0 ? "0đ" : string.Format("{0:N0}đ", amount);
            string frontendUrl = _configuration["FrontendSettings:BaseUrl"] ?? "http://localhost:44384";
            frontendUrl = frontendUrl.TrimEnd('/');

            // 3. TẠO PROMPT (BỔ SUNG LỊCH SỬ CHAT VÀO ĐÂY)

            // Format lịch sử chat thành text cho AI đọc
            string chatHistoryText = "";
            if (lichSuChat.Any())
            {
                chatHistoryText = "LỊCH SỬ TRÒ CHUYỆN VỪA QUA:\n" +
                                  string.Join("\n", lichSuChat.Select(x => $"User: {x.CauHoi}\nBot: {x.CauTraLoi}"));
            }

            string productContext = string.Join("\n", data.SanPhamShopDangBan.Select(p =>
                $"- (ID: {p.MaSanPham}) [{p.DanhMuc}] {p.TenSanPham}: {Fmt(p.Gia)}"
            ));

            // [PROMPT NÂNG CẤP]: Kèm theo lịch sử chat
            string prompt = $@"
            Bạn là Trợ lý AI 'WEBPC Bot'.
            
            1. NGỮ CẢNH HỘI THOẠI (AI cần nhớ cái này để trả lời tiếp nối):
            {chatHistoryText}

            2. HỒ SƠ TÀI CHÍNH KHÁCH HÀNG:
            - Tổng chi: {Fmt(data.TongTienDaChi)}. Đang chờ: {Fmt(data.TongTienDangCho)}.

            3. DANH SÁCH SẢN PHẨM CỬA HÀNG:
            {productContext}

            4. CÂU HỎI MỚI CỦA KHÁCH: ""{userMessage}""

            YÊU CẦU:
            - Trả lời tiếp nối câu chuyện trong lịch sử (nếu có).
            - Nếu gợi ý sản phẩm, dùng link Markdown: [Tên Sản Phẩm]({frontendUrl}/Product/Detail/{{id}})
            - Ngắn gọn, thân thiện.
            ";

            // 4. GỌI API GEMINI (Code cũ giữ nguyên logic Retry)
            string botAnswer = "Xin lỗi, hệ thống đang bận.";

            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return "Lỗi Key.";
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };

            // ... (Đoạn code gọi HTTP Client & Retry giữ nguyên như bài trước) ...
            // Thầy viết vắn tắt đoạn gọi API để tập trung vào logic lưu DB bên dưới
            // Em hãy dùng lại đoạn vòng lặp for retry của bài trước nhé!

            // [GIẢ LẬP GỌI API THÀNH CÔNG ĐỂ VIẾT TIẾP CODE LƯU DB]
            // Trong code thật của em, biến botAnswer sẽ lấy từ response của Google
            try
            {
                // -- COPY ĐOẠN GỌI API TỪ BÀI TRƯỚC VÀO ĐÂY --
                // Khi có kết quả thì gán vào botAnswer
                // Ví dụ: botAnswer = await CallGoogleApi(...);

                // Code mẫu gọi API (bản rút gọn):
                using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var respString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(respString);
                    if (doc.RootElement.TryGetProperty("candidates", out var c) && c.GetArrayLength() > 0)
                        botAnswer = c[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                }
            }
            catch (Exception ex) { botAnswer = "Lỗi kết nối AI: " + ex.Message; }


            // 5. LƯU CÂU TRẢ LỜI VÀO DB (QUAN TRỌNG)
            // -----------------------------------------------------------
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

                // Cập nhật thời gian hoạt động để reset bộ đếm 10 phút
                phienChat.ThoiGianCapNhat = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            // -----------------------------------------------------------

            return botAnswer;
        }

        // [MỚI] HÀM LẤY LỊCH SỬ CHAT
        public async Task<List<ChatMessageResponse>> GetChatHistoryAsync(int maKhachHang)
        {
            // 1. Tìm phiên chat của khách
            var phienChat = await _context.PhienChats
                .Include(p => p.ChiTietPhienChats)
                .FirstOrDefaultAsync(p => p.MaKhachHang == maKhachHang);

            // 2. Nếu không có phiên nào -> Trả về rỗng
            if (phienChat == null) return new List<ChatMessageResponse>();

            // 3. Kiểm tra hết hạn (Quá 10 phút không hoạt động)
            if ((DateTime.Now - phienChat.ThoiGianCapNhat).TotalMinutes > 10)
            {
                // Hết hạn -> Xóa phiên cũ đi cho sạch DB
                _context.PhienChats.Remove(phienChat);
                await _context.SaveChangesAsync();
                return new List<ChatMessageResponse>(); // Trả về rỗng (như mới)
            }

            // 4. Nếu còn hạn -> Convert dữ liệu DB sang List tin nhắn
            var history = new List<ChatMessageResponse>();

            foreach (var item in phienChat.ChiTietPhienChats.OrderBy(x => x.ThoiGian))
            {
                // Tin nhắn của khách
                history.Add(new ChatMessageResponse
                {
                    Role = "user",
                    Content = item.CauHoi,
                    Time = item.ThoiGian
                });

                // Tin nhắn của Bot
                history.Add(new ChatMessageResponse
                {
                    Role = "model",
                    Content = item.CauTraLoi,
                    Time = item.ThoiGian.AddSeconds(1) // Cộng 1s để nó nằm sau câu hỏi khi sort
                });
            }

            return history;
        }
    }
}