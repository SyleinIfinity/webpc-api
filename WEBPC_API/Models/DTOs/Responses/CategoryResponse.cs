namespace WEBPC_API.Models.DTOs.Responses
{
    public class CategoryResponse
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public string MoTa { get; set; }

        // Bổ sung thêm thông tin hữu ích: Số lượng sản phẩm đang có trong danh mục này
        public int SoLuongSanPham { get; set; }
    }
}