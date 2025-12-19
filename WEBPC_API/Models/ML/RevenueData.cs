// File: WEBPC_API/Models/ML/RevenueData.cs
using System;

namespace WEBPC_API.Models.ML
{
    public class RevenueData
    {
        // Thời gian (Ngày bán hàng)
        public DateTime NgayBan { get; set; }

        // Tổng tiền bán được trong ngày đó (Feature quan trọng nhất để dự báo)
        public float TongTien { get; set; }
    }
}