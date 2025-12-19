// File: WEBPC_API/Models/ML/RevenuePrediction.cs
using Microsoft.ML.Data;

namespace WEBPC_API.Models.ML
{
    public class RevenuePrediction
    {
        // Kết quả dự báo sẽ trả về một mảng số thực
        // Ví dụ: Dự báo 5 ngày tới thì mảng này có 5 phần tử
        [ColumnName("Score")]
        public float[] ForecastedRevenue { get; set; }

        // Cận dưới và cận trên của dự báo (để vẽ biểu đồ miền tin cậy - optional)
        [ColumnName("LowerBound")]
        public float[] LowerBound { get; set; }

        [ColumnName("UpperBound")]
        public float[] UpperBound { get; set; }
    }
}