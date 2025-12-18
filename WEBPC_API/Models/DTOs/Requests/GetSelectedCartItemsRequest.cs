using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class GetSelectedCartItemsRequest
    {
        [Required]
        public int MaKhachHang { get; set; }

        // Danh sách MaChiTietGioHang mà người dùng đã tick chọn
        [Required]
        public List<int> SelectedCartItemIds { get; set; }
    }
}