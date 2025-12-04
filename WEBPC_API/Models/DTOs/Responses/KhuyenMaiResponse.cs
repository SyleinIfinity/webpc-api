namespace WEBPC_API.Models.DTOs.Responses
{
    public class KhuyenMaiResponse
    {
        public int MaKhuyenMai { get; set; }
        public string MaCodeKM { get; set; }
        public string TenChuongTrinh { get; set; }
        public decimal GiaTriGiam { get; set; }
        public string LoaiGiam { get; set; }
        public decimal DonHangToiThieu { get; set; }
        public decimal? GiamToiDa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SoLuongConLai { get; set; }
    }
}