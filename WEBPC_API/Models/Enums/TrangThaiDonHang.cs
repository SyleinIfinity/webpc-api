namespace WEBPC_API.Models.Enums
{
    public enum TrangThaiDonHang
    {
        ChoXacNhan,    // Mới đặt
        ChoThanhToan,  // Chờ thanh toán (nếu chọn chuyển khoản)
        DaThanhToan,   // Đã thanh toán thành công
        DangGiao,      // Đang giao hàng
        HoanThanh,     // Giao thành công
        Huy,           // Đã hủy (Nếu chưa thanh toán)
        ChoHoanTien    // Chờ hoàn tiền (Nếu đã thanh toán mà bị hủy)
    }
}