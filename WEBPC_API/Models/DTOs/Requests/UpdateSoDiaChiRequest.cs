namespace WEBPC_API.Models.DTOs.Requests
{
    public class UpdateSoDiaChiRequest
    {
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiCuThe { get; set; }
        public string TinhThanhId { get; set; }
        public string QuanHuyenId { get; set; }
        public string PhuongXaId { get; set; }
        public bool? MacDinh { get; set; }
    }
}
