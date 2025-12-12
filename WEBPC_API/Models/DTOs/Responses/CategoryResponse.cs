namespace WEBPC_API.Models.DTOs.Responses
{
    public class CategoryResponse
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public string MoTa { get; set; }

        // --- BỔ SUNG ---
        public int? MaDanhMucCha { get; set; }
        public string? TenDanhMucCha { get; set; } // Hiển thị tên cha cho dễ nhìn

        // Bonus: Số lượng con (nếu cần)
        public int SoLuongDanhMucCon { get; set; }
    }
}